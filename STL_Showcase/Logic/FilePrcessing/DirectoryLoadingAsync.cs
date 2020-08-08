using STL_Showcase.Data.Cache;
using STL_Showcase.Data.Config;
using STL_Showcase.Logic.Files;
using STL_Showcase.Logic.Rendering;
using STL_Showcase.Shared.Enums;
using STL_Showcase.Shared.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.VisualBasic.Devices;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using STL_Showcase.Shared.Util;
using System.Collections;
using System.Windows.Media;

namespace STL_Showcase.Logic.FilePrcessing
{
    class DirectoryLoadingAsync
    {
        static NLog.Logger logger = NLog.LogManager.GetLogger("Loader");
        Thread[] MainRenderingThreads;

        private const float AvalilableMemoryMultiplier = 0.75f;
        private const int LoadFileBytesInMemoryThresholdInMB = 100; // From this size and above, do not load full file data in memory.
        private const int IntervalMS = 100;
        private const int IntervalSmallMS = 25;
        private const int SmallestThumnailSize = 32;

        private int MaxRenderingThreadsActive = 1;
        private float MaxMemoryUsedByLoadedFilesMB = 1000; // Keep loading files in memory until reaching this limit.
        private float MaxMemoryUsedByFilesMB = 100; // Do not load files that are bigger than this.
        private int[] thumbnailImagesSizes;
        private int FilesReady = 0;

        private readonly string[] SupportedExtensionsFilter = { "*.STL", "*.OBJ", "*.3MF" };

        public Action ProcessCanceledEvent;
        public Action<ModelFileData, BitmapSource[], LoadResultEnum> FileReadyEvent;
        public Action<int> ReportProgressEvent;

        IThumbnailCache cache;
        IUserSettings userSettings;

        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;

        RenderAspectEnum renderType;

        Dispatcher MainDispatcher;

        public ModelFileData[] FilesFound { get; private set; }

        public bool IsLoading { get; private set; }

        readonly ComputerInfo systemInfo = new ComputerInfo();

        ConcurrentQueue<ModelFileData> ReadyToCacheCheckList = new ConcurrentQueue<ModelFileData>();
        readonly ConcurrentQueue<ModelFileData> ReadyToLoadBasicFileDataList = new ConcurrentQueue<ModelFileData>();
        readonly ConcurrentQueue<ModelFileData> ReadyToLoadBytesList = new ConcurrentQueue<ModelFileData>();
        readonly ConcurrentQueue<ModelFileData> ReadyToRenderList = new ConcurrentQueue<ModelFileData>();
        readonly ConcurrentQueue<ModelFileData> ReadyToSaveList = new ConcurrentQueue<ModelFileData>();

        readonly ConcurrentDictionary<string, Tuple<BitmapSource, bool>[]> LoadedThumbnails = new ConcurrentDictionary<string, Tuple<BitmapSource, bool>[]>();
        int FilesBeingProcessed = 0;
        int FilesToProcess = 0;

        private void CalculateThumnailSizes(int filesAmount)
        {

            int[] availableThumnailSizes = { 182, 256, 362 }; // 182, 256, 362, 512
            if (userSettings.GetSettingBool(UserSettingEnum.EnableReduceThumbnailResolution))
                availableThumnailSizes = availableThumnailSizes.Take(availableThumnailSizes.Length / 2).ToArray();

            float availableMemoryMB = GetAvailablePhysicalMemoryMB();

            float memoryMBPerFile = (availableMemoryMB * 0.50f) / filesAmount;
            int optimalResolutionPerFile = (int)Math.Sqrt(memoryMBPerFile * 1024f * 1024f / 24f);

            var BigCalculatedThumnailSize = availableThumnailSizes[0];
            var MiddleCalculatedThumnailSize = availableThumnailSizes[0];

            if (optimalResolutionPerFile >= availableThumnailSizes[0])
            {
                for (int i = availableThumnailSizes.Length - 1; i >= 0; i--)
                {
                    if (optimalResolutionPerFile >= availableThumnailSizes[i])
                    {
                        BigCalculatedThumnailSize = availableThumnailSizes[i];
                        MiddleCalculatedThumnailSize = availableThumnailSizes[i - 4 >= 0 ? i - 4 : 0];
                        break;
                    }
                }
            }

            if (BigCalculatedThumnailSize > MiddleCalculatedThumnailSize)
                thumbnailImagesSizes = new int[] { SmallestThumnailSize, MiddleCalculatedThumnailSize, BigCalculatedThumnailSize };
            else
                thumbnailImagesSizes = new int[] { SmallestThumnailSize, BigCalculatedThumnailSize };
        }


        public bool LoadDirectory(string path)
        {
            return LoadDirectories(new string[] { path });
        }
        public bool LoadDirectories(IEnumerable<string> paths)
        {
            IsLoading = false;
            MainDispatcher = Dispatcher.CurrentDispatcher;
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            cache = DefaultFactory.GetDefaultThumbnailCache();
            userSettings = DefaultFactory.GetDefaultUserSettings();
            renderType = (RenderAspectEnum)userSettings.GetSettingInt(UserSettingEnum.Thumbnails3DAspect);

            paths = paths.Where(p1 => !paths.Any(p2 => !p1.Equals(p2) && p1.Contains(p2))).ToArray(); // Remove selected subdirectories of other selected paths.

            try
            {
                IEnumerable<string> pathsFound = new List<string>();
                for (int i = 0; i < SupportedExtensionsFilter.Length; i++)
                    foreach (string path in paths)
                    {
                        pathsFound = pathsFound.Concat(UtilMethods.EnumerateFiles(path, SupportedExtensionsFilter[i], SearchOption.AllDirectories, cancellationToken));
                    }
                pathsFound = pathsFound.ToArray();

                if (cancellationToken.IsCancellationRequested)
                    return false;

                if (pathsFound.Count() > 0)
                {
                    CalculateThumnailSizes(pathsFound.Count());
                    InitializeFoundFilesObjects(pathsFound);
                    return true;
                }
                else
                {
                    FilesFound = new ModelFileData[0];
                }
            }
            catch (Exception ex)
            {
                logger.Trace(ex, "Unable to load: {ex}", ex.Message);
            }
            return false;
        }
        public void StartLoading()
        {
            if (IsLoading)
                return;
            try
            {
                Task.Factory.StartNew(LoadFromCacheTask, cancellationTokenSource.Token);
                Task.Factory.StartNew(LoadingBasicFileDataAsync, cancellationTokenSource.Token);
                Task.Factory.StartNew(LoadingProcessAsync, cancellationTokenSource.Token);
                Task.Factory.StartNew(LoadingProcessAsync, cancellationTokenSource.Token);
                Task.Factory.StartNew(LoadingProcessAsync, cancellationTokenSource.Token);
                Task.Factory.StartNew(SaveCacheFileTask, cancellationTokenSource.Token);

                MainRenderingThreads = new Thread[MaxRenderingThreadsActive];
                for (int i = 0; i < MaxRenderingThreadsActive; i++)
                {
                    MainRenderingThreads[i] = new Thread(new ThreadStart(new Action(() => RenderProcessAsync())));
                    MainRenderingThreads[i].SetApartmentState(ApartmentState.STA);
                    MainRenderingThreads[i].Start();
                }
                this.IsLoading = true;
            }
            catch (Exception ex)
            {
                IsLoading = false;
            }
        }
        #region Threaded Loading Phases

        private void InitializeFoundFilesObjects(IEnumerable<string> paths)
        {
            FilesFound = new ModelFileData[paths.Count()];
            for (int i = 0; i < paths.Count(); i++)
            {
                string path = paths.ElementAt(i);
                FilesFound[i] = new ModelFileData(path);
            }
            ReadyToCacheCheckList = new ConcurrentQueue<ModelFileData>(FilesFound);
            FilesToProcess = FilesFound.Length;
        }
        private void LoadFromCacheTask()
        {
            while (FilesPending(DirectoryLoadingStepsEnum.FileBytesLoading) && !cancellationToken.IsCancellationRequested)
            {
                if (ReadyToCacheCheckList.TryDequeue(out ModelFileData modelFile))
                {
                    Interlocked.Increment(ref FilesBeingProcessed);
                    if (!this.LoadedThumbnails.ContainsKey(modelFile.FileFullPath))
                    {
                        // Attempt to get cache images.
                        Tuple<BitmapSource, bool>[] loadedImages = new Tuple<BitmapSource, bool>[thumbnailImagesSizes.Length];

                        for (int i = 0; i < thumbnailImagesSizes.Length; i++)
                        {
                            BitmapSource loadedImage = cache.GetThumbnailImage(modelFile.FilePath, modelFile.FileName, this.renderType, thumbnailImagesSizes[i]);
                            loadedImages[i] = new Tuple<BitmapSource, bool>(loadedImage, loadedImage != null); // True means image was loaded. False if render necessary.
                        }

                        // Add images to dictionary whether they were loaded or not. If not possible, set error and continue with next path.
                        if (this.LoadedThumbnails.TryAdd(modelFile.FileFullPath, loadedImages))
                        {
                            ReadyToLoadBasicFileDataList.Enqueue(modelFile);
                        }
                        else
                        {
                            SendToReady(modelFile, LoadResultEnum.ErrorAddingThumbnailsToList);
                            continue;
                        }
                    }

                }
                if (ReadyToCacheCheckList.Count == 0)
                    break; // Break, as its the first list of all.
            }
        }
        private async void LoadingBasicFileDataAsync()
        {
            while (FilesPending(DirectoryLoadingStepsEnum.FileBasicDataLoading) && !cancellationToken.IsCancellationRequested)
            {
                if (ReadyToLoadBasicFileDataList.TryDequeue(out ModelFileData nextFile))
                {
                    if (nextFile.LoadBasicFileData())
                    {
                        if (nextFile.FileSizeMB * 1.2f < MaxMemoryUsedByFilesMB)
                        {
                            if (!(LoadedThumbnails.TryGetValue(nextFile.FileFullPath, out Tuple<BitmapSource, bool>[] tuple) && !tuple.Any(t => !t.Item2)))
                                ReadyToLoadBytesList.Enqueue(nextFile);
                            else
                                SendToReady(nextFile, LoadResultEnum.Okay);
                        }
                        else // No enough memory (probably) to process the file.
                            SendToReady(nextFile, LoadResultEnum.NotEnoughFreeMemory);
                    }
                    else // Error loading he basic file data.
                        SendToReady(nextFile, LoadResultEnum.ErrorReadingFileData);
                }
                if (ReadyToLoadBasicFileDataList.Count == 0)
                    await Task.Delay(IntervalMS);
            }
        }
        private void LoadingProcessAsync()
        {
            while (FilesPending(DirectoryLoadingStepsEnum.FileBytesLoading) && !cancellationToken.IsCancellationRequested)
            {
                if (ReadyToRenderList.Count <= 3 && ReadyToLoadBytesList.TryDequeue(out ModelFileData nextFile))
                {
                    // Loading file
                    if (nextFile.LoadFileBytes(nextFile.FileSizeMB <= LoadFileBytesInMemoryThresholdInMB))
                    {
                        if (this.cancellationToken.IsCancellationRequested) return;
                        // File parsing
                        if (nextFile.ParseFile())
                        {
                            nextFile.ReleaseData(true, false);
                            nextFile.Mesh.CalculateFaceNormals(false);
                            this.CleanMemory();
                            if (this.cancellationToken.IsCancellationRequested) return;
                            ReadyToRenderList.Enqueue(nextFile);
                        }
                        else // Can't parse the file.
                            SendToReady(nextFile, LoadResultEnum.ErrorParsing);
                    }
                    else // Can't load file.
                        SendToReady(nextFile, LoadResultEnum.ErrorReadingFileBytes);
                }
            }
            if (ReadyToLoadBytesList.Count == 0 || ReadyToRenderList.Count > 0)
                Task.Delay(IntervalMS).Wait();
        }
        private void RenderProcessAsync()
        {
            IRenderEnviorement renderer = DefaultFactory.GetDefaultRenderEnviorement(thumbnailImagesSizes[thumbnailImagesSizes.Length - 1]);
            renderer.SetEnviorementOptions((RenderAspectEnum)userSettings.GetSettingInt(UserSettingEnum.Thumbnails3DAspect));
            while (FilesPending(DirectoryLoadingStepsEnum.FileRendering) && !cancellationToken.IsCancellationRequested)
            {
                if (ReadyToRenderList.TryDequeue(out ModelFileData nextFile))
                {
                    // File render
                    try
                    {
                        nextFile.ReleaseData(true, false);
                        renderer.SetModel(nextFile.Mesh);
                        nextFile.ReleaseData(true, true);
                        this.CleanMemory();

                        if (LoadedThumbnails.TryGetValue(nextFile.FileFullPath, out Tuple<BitmapSource, bool>[] cacheImages))
                        {

                            BitmapSource rendered = renderer.RenderImage();
                            if (rendered == null)
                            {
                                LoadedThumbnails.TryRemove(nextFile.FileFullPath, out cacheImages);
                                cacheImages = null;
                            }
                            if (cacheImages != null)
                            {
                                for (int i = 0; i < cacheImages.Length; i++)
                                {
                                    if (cacheImages[i].Item1 != null && cacheImages[i].Item2) continue; // Image exists and render is not needed.

                                    BitmapSource scaledBitmap = rendered;
                                    if (i < cacheImages.Length - 1)
                                    {
                                        double scaleFactor = (double)thumbnailImagesSizes[i] / (double)thumbnailImagesSizes[thumbnailImagesSizes.Length - 1];
                                        scaledBitmap = new TransformedBitmap(rendered, new ScaleTransform(scaleFactor, scaleFactor));
                                        scaledBitmap.Freeze();
                                    }
                                    cacheImages[i] = new Tuple<BitmapSource, bool>(scaledBitmap, false);
                                }

                                ReadyToSaveList.Enqueue(nextFile);
                            }
                            else
                                SendToReady(nextFile, LoadResultEnum.ErrorRendering);
                        }
                        else // Cant retrieve the cache.
                            SendToReady(nextFile, LoadResultEnum.ErrorRetrievingLoadedCache);
                        renderer.RemoveModel();
                    }
                    catch
                    {
                        SendToReady(nextFile, LoadResultEnum.ErrorRendering);
                    }
                    CleanMemory();
                }
                if (ReadyToRenderList.Count == 0)
                    Task.Delay(IntervalMS).Wait();
            }
        }
        private async void SaveCacheFileTask()
        {
            while (FilesPending(DirectoryLoadingStepsEnum.CacheSaving) && !cancellationToken.IsCancellationRequested)
            {
                if (ReadyToSaveList.TryDequeue(out ModelFileData nextFile))
                {
                    Tuple<BitmapSource, bool>[] cacheImages = null;
                    if (LoadedThumbnails.TryGetValue(nextFile.FileFullPath, out cacheImages))
                    {
                        bool SavingError = false;
                        for (int i = 0; i < cacheImages.Length; i++)
                        {
                            if (!cacheImages[i].Item2 && !cache.UpdateThumnail(nextFile.FilePath, nextFile.FileName, renderType, thumbnailImagesSizes[i], cacheImages[i].Item1))
                            { // Error saving the cache file.
                                SavingError = true;
                                SendToReady(nextFile, LoadResultEnum.ErrorSavingCacheFile);
                                break;
                            }
                        }
                        if (!SavingError)
                            SendToReady(nextFile, LoadResultEnum.Okay);
                    }
                    else // Error retrieving the rendered cache image.
                        SendToReady(nextFile, LoadResultEnum.ErrorRetrievingRenderedCache);
                }
                if (ReadyToSaveList.Count == 0)
                    await Task.Delay(IntervalMS);
            }
        }
        private void CleanMemory()
        {
            //_ = MainDispatcher.BeginInvoke(new Action(() =>
            //{
            GC.Collect();
            //}));
        }

        #endregion

        private bool FilesPending(DirectoryLoadingStepsEnum step)
        {
            if (FilesToProcess > 0 || FilesBeingProcessed > 0) return true;
            if (step >= DirectoryLoadingStepsEnum.CacheSaving && ReadyToSaveList.Count > 0) return true;
            if (step >= DirectoryLoadingStepsEnum.FileRendering && ReadyToRenderList.Count > 0) return true;
            if (step >= DirectoryLoadingStepsEnum.FileBytesLoading && ReadyToLoadBytesList.Count > 0) return true;
            if (step >= DirectoryLoadingStepsEnum.FileBasicDataLoading && ReadyToLoadBasicFileDataList.Count > 0) return true;
            if (step >= DirectoryLoadingStepsEnum.CacheCheck && ReadyToCacheCheckList.Count > 0) return true;
            return false;
        }

        private void SendToReady(ModelFileData fileData, LoadResultEnum result)
        {
            Interlocked.Decrement(ref FilesBeingProcessed);
            Interlocked.Decrement(ref FilesToProcess);
            Interlocked.Increment(ref FilesReady);

            fileData.ReleaseData(true, true);

            LoadedThumbnails.TryGetValue(fileData.FileFullPath, out Tuple<BitmapSource, bool>[] cacheImages);

            this.FileReadyEvent?.Invoke(fileData, cacheImages?.Select(ci => ci.Item1).ToArray(), result);
            this.ReportProgressEvent?.Invoke(FilesReady);

            if (FilesToProcess == 0 && FilesBeingProcessed == 0)
                IsLoading = false;

            //if (result != LoadResultEnum.Okay)
            //    MainDispatcher.Invoke(new Action(() =>
            //    {
            //        Console.WriteLine($"READY: {fileData.FileFullPath}, RESULT: {result.ToString()} INFO: CahceCheck: {ReadyToCacheCheckList.Count}, Load: {ReadyToLoadBytesList.Count}, Being processed:: {FilesBeingProcessed}, Save: {ReadyToSaveList.Count}");
            //    }));
        }
        /*
         Initialization: renderEnv, cache.
         Get all paths in the directory.
         Fill the pending file path list.
         Start threads:
            Cache checker.
                Runs while the path list is not empty.
                Checks if the file has the needed cache files.
                If so, loads them and the file to the ready list.
                If not, adds the file to the loading pending list.
            File loading.
                While pending loading or previous list have items, loads the file in memory.
                If its too big, loads it as a stream.
                Wait if loading the file will go over the ram threshold.
            File parsing.
                While ready to parse or previous list have items, parses the files.
                If error, adds the file to the ready list.
                If success, adds the file to render.
            File rendering.
                While ready to render or any previous list have items, render the available items.
                If error, adds the file to the error list.
                If success, add the file to cache save list.
            File cache save.
                While ready to cache or any previous list have items, save the available items.
                If fails, add a default error image.
                Any case, adds the file to ready list.
            Ready dispatcher.
         */

        /// <summary>
        /// Cancels all pending operations and executed cancelled event.
        /// </summary>
        public void CancelOperation()
        {
            IsLoading = false;
            logger.Info("Operation Cancelled");
            cancellationTokenSource.Cancel();
            ProcessCanceledEvent?.Invoke();
        }

        private float GetAvailablePhysicalMemoryMB()
        {
            return (float)(systemInfo.AvailablePhysicalMemory / (ulong)1024 / (ulong)1024) * AvalilableMemoryMultiplier;
        }

    }
}
