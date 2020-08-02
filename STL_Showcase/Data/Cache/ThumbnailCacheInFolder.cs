using STL_Showcase.Data.Config;
using STL_Showcase.Shared.Enums;
using STL_Showcase.Shared.Main;
using STL_Showcase.Shared.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static STL_Showcase.Shared.Enums.CacheEnums;

namespace STL_Showcase.Data.Cache
{
    class ThumbnailCacheInFolder : IThumbnailCache
    {

        #region Fields

        public static ThumbnailCacheInFolder Instance { get; private set; }
        static NLog.Logger logger = NLog.LogManager.GetLogger("Cache");
        const string CacheFolderName = "STLShowcaseImageCache";
        IUserSettings settings;

        #endregion

        #region Constructors

        static ThumbnailCacheInFolder()
        {
            Instance = new ThumbnailCacheInFolder();
        }

        private ThumbnailCacheInFolder()
        {
            settings = DefaultFactory.GetDefaultUserSettings();
        }

        #endregion

        #region IThumbnailCache members

        public long CacheSize()
        {
            long size = 0;
            string cachePath = GetCachePath();

            try
            {
                logger.Info("Calculating cache size...");
                if (Directory.Exists(cachePath))
                {
                    foreach (var filePath in UtilMethods.EnumerateFiles(cachePath, "*.png", SearchOption.AllDirectories))
                    {
                        using (var file = File.OpenRead(filePath))
                        {
                            size += file.Length;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Trace(ex, "Exception calculating cache size");
                return -1;
            }
            logger.Info("Calculated cache size: {size}", size);

            return size;
        }

        public bool ClearCache()
        {
            string cachePath = GetCachePath();
            bool allFilesCleared = true;

            try
            {
                if (Directory.Exists(cachePath))
                {
                    var files = UtilMethods.EnumerateFiles(cachePath, "*.png", SearchOption.AllDirectories);
                    logger.Info("Clearing cache. Detected {fileCount} files.", files.Count());

                    foreach (var filePath in files)
                    {
                        try
                        {
                            File.Delete(filePath);
                        }
                        catch (Exception ex)
                        {
                            allFilesCleared = false;
                            logger.Trace(ex, "Error deleting cache file: {filePath}", filePath);
                        }
                    }
                }
                else
                {
                    logger.Info("Clearing cache... Cache path not found! {cachePath}", cachePath);
                }
            }
            catch (Exception ex)
            {
                logger.Trace(ex, "Exception calculating cache size");
                return false;
            }
            return allFilesCleared;
        }

        public bool ClearCacheForFiles(IEnumerable<string> filenames)
        {
            string cachePath = GetCachePath();
            bool allFilesCleared = true;

            try
            {
                if (Directory.Exists(cachePath))
                {
                    var files = UtilMethods.EnumerateFiles(cachePath, "*.png", SearchOption.AllDirectories);
                    files = files.Where(f => filenames.Any(n => f.Contains(n)));
                    logger.Info("Clearing cache for specific files. Detected {fileCount} files.", files.Count());

                    foreach (var filePath in files)
                    {
                        try
                        {
                            File.Delete(filePath);
                        }
                        catch (Exception ex)
                        {
                            allFilesCleared = false;
                            logger.Trace(ex, "Error deleting cache for specific file: {filePath}", filePath);
                        }
                    }
                }
                else
                {
                    logger.Info("Clearing cache for specific files... Cache path not found! {cachePath}", cachePath);
                }
            }
            catch (Exception ex)
            {
                logger.Trace(ex, "Exception calculating cache size for specific files");
                return false;
            }
            return allFilesCleared;

        }
        public IEnumerable<Tuple<int, BitmapSource>> GetThumbnailsImages(string filePath, string fileName, RenderAspectEnum renderType)
        {
            Tuple<int, BitmapSource>[] loadedFiles = new Tuple<int, BitmapSource>[0];
            string cachePath = GetCachePath();
            string filesNameForFilter = GetComposedFileNameForFilter(fileName, renderType);

            try
            {
                if (Directory.Exists(cachePath))
                {
                    var files = UtilMethods.EnumerateFiles(cachePath, filesNameForFilter, SearchOption.AllDirectories);
                    loadedFiles = new Tuple<int, BitmapSource>[files.Count()];
                    int i = 0;
                    foreach (var foundFile in files)
                    {
                        using (var file = File.OpenRead(foundFile))
                        {
                            BitmapSource bmp = LoadImageFromStream(file);
                            loadedFiles[i] = new Tuple<int, BitmapSource>(GetFileSizeFromFileName(foundFile), bmp);
                        }
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Trace(ex, "Exception loading cache file images for {fullpath} as {renderType}", Path.Combine(filePath, fileName), renderType.ToString());
            }

            if (loadedFiles.Length == 0) logger.Info("No cache files found for {filePath} as {filesNameForFilter}", Path.Combine(filePath, fileName), filesNameForFilter);
            else logger.Info("Loaded {fileCount} cache files found for {filePath} as {filesNameForFilter}", loadedFiles.Length, Path.Combine(filePath, fileName), filesNameForFilter);

            return loadedFiles;
        }
        public BitmapSource GetThumbnailImage(string filePath, string fileName, RenderAspectEnum renderType, int thumbnailSize)
        {
            BitmapSource loadedFile = null;
            string cachePath = GetCachePath(thumbnailSize);
            string thumbnailFileName = ComposeFileName(fileName, renderType, thumbnailSize);
            string fullThumbnailFilePath = Path.Combine(cachePath, thumbnailFileName);
            try
            {
                if (Directory.Exists(cachePath) && File.Exists(fullThumbnailFilePath))
                {
                    using (var file = File.OpenRead(fullThumbnailFilePath))
                    {
                        loadedFile = LoadImageFromStream(file);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Trace(ex, "Exception loading cache file images for {fullpath} as {renderType}", Path.Combine(filePath, fileName), renderType.ToString());
                return null;
            }

            if (loadedFile == null) logger.Info("Cache file not found at {FullThumbnailFilePath}", fullThumbnailFilePath);
            else logger.Info("Loaded cache file at {FullThumbnailFilePath}", fullThumbnailFilePath);

            return loadedFile;
        }
        public IEnumerable<Tuple<int, string>> GetThumbnailsPaths(string filePath, string fileName, RenderAspectEnum renderType)
        {
            Tuple<int, string>[] foundFiles = null;
            string cachePath = GetCachePath();
            string filesNameForFilter = GetComposedFileNameForFilter(fileName, renderType);

            try
            {
                if (Directory.Exists(cachePath))
                {
                    var files = UtilMethods.EnumerateFiles(cachePath, filesNameForFilter, SearchOption.AllDirectories);
                    foundFiles = files.Select(f => new Tuple<int, string>(GetFileSizeFromFileName(f), f)).ToArray();
                }
            }
            catch (Exception ex)
            {
                logger.Trace(ex, "Exception loading cache file paths for {fullpath} as {renderType}", Path.Combine(filePath, fileName), renderType.ToString());
                return null;
            }

            if (foundFiles == null || foundFiles.Length == 0) logger.Info("No cache files found for {filePath} as {filesNameForFilter}", Path.Combine(filePath, fileName), filesNameForFilter);
            else logger.Info("Found {fileCount} cache files for {filePath} as {filesNameForFilter}", foundFiles.Length, Path.Combine(filePath, fileName), filesNameForFilter);

            return foundFiles;
        }

        public bool UpdateThumnail(string filePath, string fileName, RenderAspectEnum renderType, int size, BitmapSource img)
        {
            bool updated = true;
            string cachePath = GetCachePath(size);
            string cachedFileName = ComposeFileName(fileName, renderType, size);
            string fullCachedFilePath = Path.Combine(cachePath, cachedFileName);

            try
            {
                if (!Directory.Exists(cachePath))
                {
                    Directory.CreateDirectory(cachePath);
                    logger.Info("Created cache directory at {cachePath}", cachePath);
                }
            }
            catch (Exception ex)
            {
                logger.Trace(ex, "Exception creating the cache directory at  {cachePath}", cachePath);
                return false;
            }

            try
            {
                using (var file = File.OpenWrite(fullCachedFilePath))
                {
                    PngBitmapEncoder png = new PngBitmapEncoder();
                    png.Frames.Add(BitmapFrame.Create(img));
                    png.Save(file);
                }
                logger.Info("Updated file thumbnail for {fileName} for size {size}", fileName, size);
            }
            catch (Exception ex)
            {
                logger.Trace(ex, "Exception updating a file thumbnail at {fullCachedFilePath} for size {size} ", fullCachedFilePath, size);
                return false;
            }

            return updated;
        }

        public string GetCurrentCachePath()
        {
            return GetCachePath();
        }
        public bool MoveCacheToNewLocation(CachePathType newPath)
        {
            return false;
        }

        public bool CheckThumbnailExists(string filePath, string fileName, RenderAspectEnum renderType, int thumbnailSize)
        {
            string cachePath = GetCachePath();
            string thumbnailFileName = ComposeFileName(fileName, renderType, thumbnailSize);

            try
            {
                return Directory.Exists(cachePath) && File.Exists(Path.Combine(cachePath, thumbnailFileName));
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Private methods

        private BitmapSource LoadImageFromStream(Stream stm)
        {
            try
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.StreamSource = stm;
                img.EndInit();
                img.Freeze();
                return img;
            }
            catch
            {
                return null;
            }
        }

        private string GetCachePath(int size = 0)
        {
            string cachePath = "";

            CachePathType cachePathLocation = (CachePathType)settings.GetSettingInt(Shared.Enums.UserSettingEnum.PreferredCachePath);
            switch (cachePathLocation)
            {
                case CachePathType.ApplicationFolder:
                    cachePath = Path.Combine(AppContext.BaseDirectory, CacheFolderName);
                    break;
                case CachePathType.UserDataFolder:
                    cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), CacheFolderName);
                    break;
                case CachePathType.UserImagesFolder:
                    cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), CacheFolderName);
                    break;
            }

            if (size > 0)
                cachePath = Path.Combine(cachePath, size.ToString());
            logger.Info("Using cache folder {cachePath}", cachePath);

            return cachePath;
        }

        private string ComposeFileName(string fileName, RenderAspectEnum renderType, int size)
        {
            return string.Format($"{fileName}.cached.{(int)renderType}.{size}.png");
        }
        private string GetComposedFileNameForFilter(string fileName, RenderAspectEnum renderType)
        {
            return string.Format($"{fileName}.cached.{(int)renderType}.*.png");
        }
        private int GetFileSizeFromFileName(string fileName)
        {
            if (int.TryParse(Regex.Match(fileName, @"\.\d+\.(\d+)\.png*").Groups[1].Value, out int matched))
                return matched;
            return 0;
        }

        #endregion
    }
}
