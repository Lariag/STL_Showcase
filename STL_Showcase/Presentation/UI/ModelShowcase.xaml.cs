using Ookii.Dialogs.Wpf;
using STL_Showcase.Data.Cache;
using STL_Showcase.Logic.Files;
using STL_Showcase.Logic.Rendering;
using STL_Showcase.Presentation.UI.Clases;
using STL_Showcase.Shared.Enums;
using STL_Showcase.Shared.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using ModernWpf.Controls;
using STL_Showcase.Data.Config;
using STL_Showcase.Logic.FilePrcessing;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using STL_Showcase.Presentation.UI.Clases.Utility;

namespace STL_Showcase.Presentation.UI
{

    /// <summary>
    /// Interaction logic for ModelShowcase.xaml
    /// </summary>
    public partial class ModelShowcase : Page
    {
        #region Variables

        static NLog.Logger mainLogger = NLog.LogManager.GetLogger("Main");
        private MainWindow _w;
        private ModelItemListData _ModelItemListData;
        private ModelProgressBarData _ModelProgressBarData;
        View3D view3d;

        private ModelCacheInfo _CacheInfo;
        private Model3DViewInfo _Model3DViewInfo;
        private ModelColumnsStates ColumnModeManager;

        IUserSettings userSettings = DefaultFactory.GetDefaultUserSettings();

        DateTime lastListFilterKey = DateTime.Now;
        CancellationTokenSource listFilterCancellationTokenSource;

        DirectoryLoadingAsync CurrentDirectoryLoader;

        public BitmapImage DefaultImageErrorModel { get; set; }
        public BitmapImage DefaultImageLoading { get; set; }

        #endregion

        #region Initialization
        public ModelShowcase(MainWindow w)
        {
            InitializeComponent();

            userSettings = DefaultFactory.GetDefaultUserSettings();

            InitializeVisibilityMenuItemsCheckedState();
            UpdateVisibilityMenuItemsCheckedState();

            _ModelItemListData = new ModelItemListData();
            _ModelItemListData.ZoomLevelChanged += (sender, e) => ModelListItem.SetImageSizeFor(_ModelItemListData.ModelListItemContentSize);
            _ModelItemListData.ZoomLevelChanged += ModelItemList_ZoomLevelChanged_UpdateScrollPosition;
            //_ModelItemListData.ZoomLevelChanged += (sender, e) => SetSelectedListItemVisible;
            ImageListZoomSliderControl.DataContext = _ModelItemListData;
            ImageListControl.DataContext = _ModelItemListData;
            ImageTreeControl.DataContext = _ModelItemListData;
            ModelListOrderPanel.DataContext = _ModelItemListData;
            DirectoryTextFilterButton.DataContext = _ModelItemListData;
            _ModelItemListData.MaximumZoomLevel = ImageListZoomSliderControl.Maximum;
            TreeItemFilterButtons.DataContext = _ModelItemListData;

            _ModelProgressBarData = new ModelProgressBarData();
            ProgressBarControl.DataContext = _ModelProgressBarData;
            ProgressBarControlContainer.DataContext = _ModelProgressBarData;
            _ModelProgressBarData.ReversedMode = true;
            _ModelProgressBarData.IsLoading = false;

            listFilterCancellationTokenSource = new CancellationTokenSource();


            _CacheInfo = new ModelCacheInfo();
            MenuCacheMain.DataContext = _CacheInfo;

            var viewport = new HelixViewport3D();
            view3d = new View3D(viewport);
            view3d.OptionRenderStyle = (RenderAspectEnum)userSettings.GetSettingInt(UserSettingEnum.CurrentView3DAspect);
            ViewportContainer.Child = viewport;
            view3d.SetModel(null);
            view3d.UpdateLights();

            _Model3DViewInfo = new Model3DViewInfo();
            View3DText.DataContext = _Model3DViewInfo;
            RenderTypeScrollItemContainer.DataContext = _Model3DViewInfo;

            view3d.LoadModelInfoAvailableEvent += LoadModelInfoAvailable;

            this._w = w;
            _w.ClosingEvent += UnloadDirectory;
            InitializeResources();
            InitializeDirectoryLoading();
        }
        public void InitializeResources()
        {

            DefaultImageErrorModel = new BitmapImage(new Uri("pack://application:,,,/Presentation/UI/Styles/Resources/STL_Error.png", UriKind.Absolute));
            DefaultImageLoading = new BitmapImage(new Uri("pack://application:,,,/Presentation/UI/Styles/Resources/STL_Loading.png", UriKind.Absolute));
        }
        public void TestFileLoading_2()
        {




            try
            {
                mainLogger.Debug("TestFileLoading_2");

                var renderEnv = DefaultFactory.GetDefaultRenderEnviorement(512);
                var cache = DefaultFactory.GetDefaultThumbnailCache();


                var cacheSize = cache.CacheSize();
                int[] cacheSizes = { 32, 256 };
                RenderAspectEnum renderAspect = RenderAspectEnum.PerNormal;
                renderEnv.SetEnviorementOptions(renderAspect);

                //string[] stlFiles = Directory.EnumerateFiles( @"I:\STL_Showcase\STLs", "*.stl", SearchOption.AllDirectories ).ToArray();
                string[] stlFiles = Directory.EnumerateFiles(@"C:\Users\Alriac\Downloads\", "*.stl", SearchOption.AllDirectories).ToArray();
                // string[] stlFiles = Directory.EnumerateFiles( @"D:\Torrent\STLs\", "*.stl", SearchOption.AllDirectories ).ToArray();
                //string[] stlFiles = Directory.EnumerateFiles( @"G:\Proyectos\3DPrinting\", "*.stl", SearchOption.AllDirectories ).ToArray();

                //string[] stlFiles = Directory.EnumerateFiles( @"D:\TestSTL", "*.stl", SearchOption.AllDirectories ).ToArray();

                //string[] stlFiles =
                //{
                //    @"G:\Proyectos\3DPrinting\dodecaedro.stl",
                //    @"G:\Proyectos\3DPrinting\botella.stl",
                //    @"C:\Users\Alriac\Downloads\old\3DBenchy.stl",
                //    @"C:\Users\Alriac\Downloads\old\xyzCalibration_cube.stl"
                //};

                double[] times = new double[stlFiles.Length];
                //var res = Parallel.ForEach( stlFiles, (path) => {

                var newModelList = new List<ModelListItem>();

                foreach (string path in stlFiles)
                {
                    DateTime currentTime = DateTime.Now;
                    bool fileTooBig = false;

                    var fileData = new ModelFileData(path);
                    fileData.LoadBasicFileData();
                    var modelListItem = new ModelListItem();
                    modelListItem.FileData = fileData;

                    // continue;

                    List<Tuple<int, BitmapSource>> cacheImages = new List<Tuple<int, BitmapSource>>(cache.GetThumbnailsImages(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileName(path), renderAspect));


                    if (cacheSizes.Length != cacheImages.Count())
                    {
                        if (fileData.LoadFileBytes(true) && fileData.ParseFile())
                        {
                            fileData.ReleaseData(true, false);
                            GC.Collect();
                            renderEnv.SetModel(fileData.Mesh);

                            foreach (var imageSize in cacheSizes)
                            {
                                if (cacheImages.Any(c => c.Item1 == imageSize))
                                    continue;

                                BitmapSource image = renderEnv.RenderImage();
                                cache.UpdateThumnail(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileName(path), renderAspect, imageSize, image);
                                cacheImages.Add(new Tuple<int, BitmapSource>(imageSize, image));
                            }
                            renderEnv.RemoveModel();
                            //using(Stream stm = File.Create( System.IO.Path.Combine( @"G:\Proyectos\3DPrinting\TestRenderer", fileData.FileName + ".png" ) )) {
                            //    stm.Write( image, 0, image.Length );
                            //}
                        }
                        else
                        {
                            foreach (var imageSize in cacheSizes)
                            {
                                if (cacheImages.Any(c => c.Item1 == imageSize))
                                    continue;
                                BitmapSource image = DefaultImageErrorModel;
                                //if (fileTooBig)
                                //{
                                //    image = DefaultImageModelTooBig;
                                //}
                                //else
                                //{
                                //    image = DefaultImageErrorModel;
                                //}
                                // cache.UpdateThumnail( System.IO.Path.GetDirectoryName( path ), System.IO.Path.GetFileName( path ), renderAspect, imageSize, image ); // Dont add to the cache!
                                cacheImages.Add(new Tuple<int, BitmapSource>(imageSize, image));
                            }
                        }
                    }

                    //if(cacheImages.Count > 0) {
                    //    string cachePath = cache.GetThumbnailsPaths( System.IO.Path.GetDirectoryName( path ), System.IO.Path.GetFileName( path ), renderAspect ).FirstOrDefault()?.Item2 ?? "";
                    //    modelListItem.ImagePath = cachePath;
                    //    modelListItem.imagesAllLevels = cacheImages.OrderBy( c => c.Item1 ).Select( c => c.Item2 ).ToArray();
                    //    ModelList.Add( modelListItem );
                    //}

                    if (cacheImages.Count > 0)
                    {
                        modelListItem.SetImages(cacheImages.OrderBy(c => c.Item1).Select(c => c.Item2).ToArray());
                        newModelList.Add(modelListItem);
                    }

                    //Console.WriteLine( ( DateTime.Now - currentTime ).TotalSeconds.ToString( "f3" ) + " file: " + fileData.FileName );
                    fileData.ReleaseData(true, true);
                    GC.Collect();
                }

                // List View
                {
                    _ModelItemListData.ModelList = new ObservableCollection<ModelListItem>(newModelList);
                    ImageListControl.ItemsSource = _ModelItemListData.ModelListFiltered;
                }

                // Tree View
                {
                    IEnumerable<Tuple<string[], object>> tuples = newModelList.OrderBy(m => m.FileData.FileName).OrderBy(m => m.FileData.FilePath).Select(m => new Tuple<string[], object>((m.FileData.FileFullPath).Split(System.IO.Path.DirectorySeparatorChar), m));

                    ModelTreeItem treeRoot = new ModelTreeItem(1, tuples.FirstOrDefault().Item1[0], null, (mfd) =>
                   {
                       return ((ModelListItem)mfd).ImageSmallest;
                   });

                    treeRoot.BuildTreeRecursive(tuples);
                    var trimmedTree = treeRoot.Trim();
                    // treeRoot.DebugListTree();

                    foreach (var node in trimmedTree)
                        node.IsExpanded = true;

                    _ModelItemListData.ModelTreeRoot = trimmedTree;
                    ImageTreeControl.ItemsSource = _ModelItemListData.ModelTreeRoot;
                }
            }
            catch (Exception ex)
            {
                var test = ex.ToString();
            }
        }

        private void InitializeDirectoryLoading()
        {
            try
            {
                string lastDirectory = userSettings.GetSettingString(UserSettingEnum.LastDirectory);
                if (!string.IsNullOrWhiteSpace(lastDirectory) && Directory.Exists(lastDirectory))
                {
                    LoadDirectory(lastDirectory);
                }
            }
            catch
            {
                userSettings.SetSettingString(UserSettingEnum.LastDirectory, "");
            }
        }

        #endregion

        #region Main Menu Events
        private void LoadDirectory_Click(object sender, RoutedEventArgs e)
        {
            LoadDirectoryWithDialog();
        }



        private void MenuItemConfiguration_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationWindow config = new ConfigurationWindow();
            //config.ShowDialog();
        }

        private void MenuCacheMain_Click(object sender, RoutedEventArgs e)
        {
            _CacheInfo.CalculateCacheSize(this.Dispatcher);
        }

        private void MenuCachePath_Click(object sender, RoutedEventArgs e)
        {
            var cache = DefaultFactory.GetDefaultThumbnailCache();
            string cachePath = cache.GetCurrentCachePath();
            try
            {
                if (string.IsNullOrWhiteSpace(cachePath) || !Directory.Exists(cachePath))
                    MessageBox.Show("The no cache folder doesn't exist yet.");
                else
                    Process.Start(cachePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Unable to open cache folder: {cachePath}", cachePath));
            }
        }

        private async void MenuCacheDelete_Click(object sender, RoutedEventArgs e)
        {
            string deleteTitle = "Delete Cache Files";
            string deleteQuestion = "The cache folder and all its contents will be deleted.\nContinue?";
            string deleteConfirmation = "All cache images were successfully deleted.";
            string deleteError = "All cache images were successfully deleted.";

            if ((await new MessageDialog(deleteQuestion, deleteTitle, "Confirm", "", "Cancel").ShowAsync()) == ContentDialogResult.Primary)
            {
                var cache = DefaultFactory.GetDefaultThumbnailCache();
                if (cache.ClearCache())
                {
                    await new MessageDialog(deleteConfirmation, deleteTitle, "OK", "", "").ShowAsync();
                }
                else
                {
                    await new MessageDialog(deleteError, deleteTitle, "OK", "", "").ShowAsync();
                }
            }
        }

        private async void MenuCacheAbout_Click(object sender, RoutedEventArgs e)
        {
            string message = "The cache folder contains the auto-generated images of your 3D models.\nThose images are generated when a new model is loaded, so the next time a image of the model is needed, it won't be necessary to read all the geometry and render it.\n\nImages will be generated in different sizes and colors depending on your settings.";
            ContentDialog dialog = new MessageDialog(message, "About the Cache...", "OK", "", "");
            await dialog.ShowAsync();
        }
        private async void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            string message = "Information about the program...";
            ContentDialog dialog = new MessageDialog(message, "About STL Showcase", "OK", "", "");
            await dialog.ShowAsync();
        }
        #endregion

        #region Model Tree Events

        private void ImageTreeControl_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ModelTreeItem newItem = e.NewValue as ModelTreeItem;
            _ModelItemListData.SelectedTreeItem = newItem;

            if (newItem != null)
            {
                if (newItem.HasData)
                {
                    _ModelItemListData.SelectedListItem = newItem.Data as ModelListItem; // Unselect/Select list item wheter or not is visilbe.
                    if (!string.IsNullOrWhiteSpace(_ModelItemListData.ModelListDirectoryFilter) && !_ModelItemListData.SelectedListItem.FileData.FilePath.Contains(_ModelItemListData.ModelListDirectoryFilter))
                    {
                        _ModelItemListData.ModelListDirectoryFilter = _ModelItemListData.SelectedListItem.FileData.FilePath;
                        ImageListControl.ItemsSource = _ModelItemListData.ModelListFiltered;
                    }
                    LoadModelInViewport(_ModelItemListData.SelectedListItem.FileData);
                }
                else
                {
                    _ModelItemListData.ModelListDirectoryFilter = string.Join(System.IO.Path.DirectorySeparatorChar + "", newItem.GetTextsToRoot());
                    ImageListControl.ItemsSource = _ModelItemListData.ModelListFiltered;
                }
                if (_ModelItemListData.SelectedListItem != null)
                    BringSelectedListItemToView();
            }
        }

        private void TreeExpandAll_Click(object sender, RoutedEventArgs e)
        {
            if (_ModelItemListData.ModelTreeRoot != null)
                foreach (var node in _ModelItemListData.ModelTreeRoot)
                {
                    node.SetIsExpandedAll(true);
                    if (node.ParentItem != null) node.ParentItem.IsExpanded = true;
                }
        }

        private void TreeContractAll_Click(object sender, RoutedEventArgs e)
        {
            if (_ModelItemListData.ModelTreeRoot != null)
                foreach (var node in _ModelItemListData.ModelTreeRoot)
                {
                    node.SetIsExpandedAll(false);
                    if (node.ParentItem != null) node.ParentItem.IsExpanded = false;
                }
        }

        private void DirectoryUnloadAll_Click(object sender, RoutedEventArgs e)
        {
            UnloadDirectory();
        }

        private void DirectoryAddOneMore_Click(object sender, RoutedEventArgs e)
        {

        }
        private void FileTypeFilterToggleButton_Click(object sender, RoutedEventArgs e)
        {
            this._ModelItemListData.ApplyFilterToTree();

            //ImageTreeControl.ItemsSource = this._ModelItemListData.ModelTreeRoot;
            ImageListControl.ItemsSource = this._ModelItemListData.ModelListFiltered;
        }
        #endregion

        #region Model List Events and Methods

        private void ImageListZoomSliderControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_ModelItemListData != null)
                this._ModelItemListData.CurrentZoomLevel = e.NewValue;
        }

        private void ImageListItemClick_Click(object sender, RoutedEventArgs e)
        {
            if ((ModelListItem)((Button)e.Source).CommandParameter == _ModelItemListData.SelectedListItem)
            {
                _ModelItemListData.SelectedListItem = null;
                LoadModelInViewport(null);
                return;
            }

            _ModelItemListData.SelectedListItem = (ModelListItem)((Button)e.Source).CommandParameter;
            LoadModelInViewport(_ModelItemListData.SelectedListItem.FileData);
        }

        private void ButtonZoomLevel_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)e.Source;
            int zoomChange = int.Parse((string)btn.Tag);
            _ModelItemListData.ChangeZoomLevel(zoomChange);
        }
        private void ModelItemList_ZoomLevelChanged_UpdateScrollPosition(object sender, EventArgs e)
        {
            double scrollHeight = ModelItemListScrollPanel.ScrollableHeight;
            double scrollPosition = ModelItemListScrollPanel.VerticalOffset;

            if (scrollHeight > 0)
            {
                this.UpdateLayout();
                ModelItemListScrollPanel.ScrollToVerticalOffset(scrollPosition / scrollHeight * ModelItemListScrollPanel.ScrollableHeight);
            }
        }
        private void ImageListZoomSliderControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _ModelItemListData.ChangeZoomLevel(e.Delta < 0 ? -1 : 1);
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                _ModelItemListData.ChangeZoomLevel(e.Delta < 0 ? -1 : 1);
            }
            else
            {
                ModelItemListScrollPanel.ScrollToVerticalOffset(ModelItemListScrollPanel.VerticalOffset - e.Delta * 3);
            }
            e.Handled = true;
        }

        private void RenderTypeScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            RenderTypeScrollViewer.ScrollToHorizontalOffset(RenderTypeScrollViewer.HorizontalOffset + e.Delta); // Add or substract... not an easy choice.
        }
        private void ModelListTextFilter_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (!listFilterCancellationTokenSource.Token.IsCancellationRequested)
                listFilterCancellationTokenSource.Cancel();

            listFilterCancellationTokenSource = new CancellationTokenSource();
            var Token = listFilterCancellationTokenSource.Token;

            Task.Factory.StartNew(new Action<object>(async (object d) =>
          {
              var dispatcher = (Dispatcher)d;
              int i = 250;
              while (!Token.IsCancellationRequested && i > 0)
              {
                  await Task.Delay(25);
                  i -= 25;
              }

              if (!Token.IsCancellationRequested)
              {
                  dispatcher.Invoke(new Action(() =>
                  {
                      _ModelItemListData.ModelListTextFilter = ((TextBox)sender).Text;
                      ImageListControl.ItemsSource = _ModelItemListData.ModelListFiltered;
                      BringSelectedListItemToView();
                  }));
              }
          }), this.Dispatcher, listFilterCancellationTokenSource.Token);


        }

        private void RadioButtonFilterModelList_Click(object sender, RoutedEventArgs e)
        {
            ImageListControl.ItemsSource = this._ModelItemListData.ModelListFiltered;
        }

        private void DirectoryTextFilterButton_Click(object sender, RoutedEventArgs e)
        {
            this._ModelItemListData.ModelListDirectoryFilter = "";
            ImageListControl.ItemsSource = _ModelItemListData.ModelListFiltered;
        }

        /// <summary>
        /// If the list of items has one selected and is not filtered out, brings it to view.
        /// </summary>
        void BringSelectedListItemToView()
        {
            if (_ModelItemListData.SelectedListItem != null && _ModelItemListData.ModelListFiltered.Contains(_ModelItemListData.SelectedListItem))
            {
                for (int i = 0; i < ImageListControl.Items.Count; i++)
                {
                    ContentPresenter c = (ContentPresenter)ImageListControl.ItemContainerGenerator.ContainerFromItem(ImageListControl.Items[i]);
                    c.ApplyTemplate();

                    Button tb = c.ContentTemplate.FindName("ImageListItemClick", c) as Button;
                    if (((ModelListItem)tb.CommandParameter) == _ModelItemListData.SelectedListItem)
                    {
                        tb.BringIntoView();
                        break;
                    }
                }
            }
        }

        #endregion

        #region Directory Loading

        private void LoadDirectoryWithDialog()
        {

            VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();
            folderDialog.RootFolder = Environment.SpecialFolder.MyComputer;
            var result = folderDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                string path = folderDialog.SelectedPath;

                userSettings.SetSettingString(UserSettingEnum.LastDirectory, path);
                LoadDirectory(path);
            }
        }
        private void UnloadDirectory()
        {
            if (CurrentDirectoryLoader != null && CurrentDirectoryLoader.IsLoading)
                CurrentDirectoryLoader.CancelOperation();
            _ModelItemListData.Reset();
            ImageListControl.ItemsSource = _ModelItemListData.ModelListFiltered;
            ImageTreeControl.ItemsSource = _ModelItemListData.ModelTreeRoot;
            GC.Collect();
        }
        private void LoadDirectoryFileReady(ModelFileData modelFile, BitmapSource[] cacheImages, LoadResultEnum result)
        {
            try
            {
                if (CurrentDirectoryLoader != null && CurrentDirectoryLoader.IsLoading)
                    _ = this.Dispatcher.BeginInvoke(() =>
                      {
                          var listItem = this._ModelItemListData.GetFromInternalDictionary(modelFile.FileFullPath);
                          if (listItem != null)
                          {
                              BitmapSource[] images = cacheImages;
                              if (result != LoadResultEnum.Okay)
                              {
                                  images = new BitmapSource[] { DefaultImageErrorModel };
                              }
                              listItem.SetImages(images);
                          }
                      });
            }
            catch
            {

            }
        }
        private void LoadDirectoryReportProgress(int p)
        {
            try
            {
                _ = this.Dispatcher.BeginInvoke(() =>
                {
                    this._ModelProgressBarData.CurrentProgress = p;
                    if (_ModelProgressBarData.CurrentProgress >= _ModelProgressBarData.MaxProgress)
                        _ModelProgressBarData.IsLoading = false;
                });
            }
            catch
            {

            }
        }
        private void LoadDirectoryCancelled()
        {
            try
            {
                _ = this.Dispatcher.BeginInvoke(() =>
                {
                    _ModelProgressBarData.IsLoading = false;
                });
            }
            catch
            {

            }
        }
        private void LoadDirectory(string dir)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            LoadingDialog loading = new LoadingDialog($"Looking for files at '{dir}'...", "Load Directory", "Cancel", () => source.Cancel(false));
            Task loadingTask = loading.ShowAsync();

            if (CurrentDirectoryLoader != null && CurrentDirectoryLoader.IsLoading)
            {
                CurrentDirectoryLoader.FileReadyEvent -= LoadDirectoryFileReady;
                CurrentDirectoryLoader.ReportProgressEvent -= LoadDirectoryReportProgress;
                CurrentDirectoryLoader.ProcessCanceledEvent -= LoadDirectoryCancelled;
                CurrentDirectoryLoader.CancelOperation();

                _ModelProgressBarData.CurrentProgress = 0;
                _ModelProgressBarData.IsLoading = false;
            }

            CurrentDirectoryLoader = new DirectoryLoadingAsync();

            //CurrentDirectoryLoader.ReportProgressEvent += new Action<int>((p) => Console.WriteLine("Progress: " + p));
            CurrentDirectoryLoader.FileReadyEvent += LoadDirectoryFileReady;
            CurrentDirectoryLoader.ReportProgressEvent += LoadDirectoryReportProgress;
            CurrentDirectoryLoader.ProcessCanceledEvent += LoadDirectoryCancelled;

            Task.Factory.StartNew(() =>
            {
                CancellationToken t = source.Token;
                try
                {
                    if (CurrentDirectoryLoader.LoadDirectory(dir))
                    {
                        if (t.IsCancellationRequested)
                            return;

                        Dispatcher.Invoke(() => { UnloadDirectory(); return true; });

                        Tuple<ModelFileData, LoadResultEnum>[] modelFiles = CurrentDirectoryLoader.FilesFound.Select(m => new Tuple<ModelFileData, LoadResultEnum>(m, LoadResultEnum.Okay)).OrderBy(f => f.Item2).ToArray();
                        // List View
                        {
                            List<ModelListItem> newModelList = new List<ModelListItem>(modelFiles.Length);
                            foreach (Tuple<ModelFileData, LoadResultEnum> fileData in modelFiles)
                            {
                                var modelListItem = new ModelListItem();
                                modelListItem.FileData = fileData.Item1;
                                modelListItem.SetImages(new BitmapSource[] { fileData.Item2 == LoadResultEnum.Okay ? DefaultImageLoading : DefaultImageErrorModel });
                                newModelList.Add(modelListItem);
                            }
                            this.Dispatcher.Invoke(() => { _ModelItemListData.ModelList = new ObservableCollection<ModelListItem>(newModelList); return true; });
                        }

                        // Tree View
                        {
                            IEnumerable<Tuple<string[], object>> tuples = _ModelItemListData.ModelList.OrderBy(m => m.FileData.FileName).OrderBy(m => m.FileData.FilePath).Select(m => new Tuple<string[], object>((m.FileData.FileFullPath).Split(System.IO.Path.DirectorySeparatorChar), m));

                            ModelTreeItem treeRoot = new ModelTreeItem(1, tuples.FirstOrDefault().Item1[0], null, (mfd) =>
                            {
                                return ((ModelListItem)mfd).ImageSmallest;
                            });

                            treeRoot.BuildTreeRecursive(tuples);
                            var trimmedTree = treeRoot.Trim();

                            foreach (var node in trimmedTree)
                                node.IsExpanded = true;

                            this.Dispatcher.Invoke(() => { _ModelItemListData.ModelTreeRoot = trimmedTree; return true; });
                        }

                        this.Dispatcher.Invoke(() =>
                        {
                            _ModelProgressBarData.IsLoading = true;
                            _ModelProgressBarData.MaxProgress = CurrentDirectoryLoader.FilesFound.Length;
                            _ModelProgressBarData.CurrentProgress = 0;
                            return true;
                        });

                        CurrentDirectoryLoader.StartLoading();
                        this.Dispatcher.Invoke(() => loading.CloseDialog());

                    }
                    else if (!t.IsCancellationRequested)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            loading.CloseDialog();

                            if (CurrentDirectoryLoader.FilesFound.Length > 0)
                                new MessageDialog("Unable to load the directory.", "Loading Error", "OK", "", "").ShowAsync();
                            else
                                new MessageDialog($"No files found at {dir}.", "Nothing found!", "OK", "", "").ShowAsync();
                            CurrentDirectoryLoader = null;
                            return true;
                        });
                    }

                    this.Dispatcher.Invoke(() =>
                    {
                        ImageListControl.ItemsSource = _ModelItemListData.ModelListFiltered;
                        ImageTreeControl.ItemsSource = _ModelItemListData.ModelTreeRoot;
                        return true;
                    });

                }
                catch (OperationCanceledException)
                {
                    this.Dispatcher.Invoke(() => { UnloadDirectory(); return true; });
                    this.Dispatcher.Invoke(() => loading.CloseDialog());
                }
                finally
                {
                    GC.Collect();
                }
            }, source.Token);

        }

        #endregion

        #region Model Viewport

        private void LoadModelInViewport(ModelFileData modelData)
        {
            this.view3d.SetModel(modelData);
        }

        private void LoadModelInfoAvailable(string name, int tris, int verts, int sizeKB)
        {
            this._Model3DViewInfo.SetData(name, tris, verts, sizeKB);

        }
        private void ButtonReset3DCamera_Click(object sender, RoutedEventArgs e)
        {
            view3d.ResetCamera();
        }

        private void ButtonTake3DScreenShot_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ToggleButton3DAutoRotation_Click(object sender, RoutedEventArgs e)
        {
            view3d.SetCameraRotationMode(((ToggleButton)sender).IsChecked ?? true);
        }


        #endregion

        private void RenderTypeButton_Click(object sender, RoutedEventArgs e)
        {
            int renderAspectInt = int.Parse(((RadioButton)sender).Tag.ToString());
            this.userSettings.SetSettingInt(UserSettingEnum.CurrentView3DAspect, renderAspectInt);
            view3d.OptionRenderStyle = (RenderAspectEnum)renderAspectInt;
            view3d.UpdateLights();
        }


        private void ColumnVisibilityOption_Click(object sender, RoutedEventArgs e)
        {
            string btnTag = (sender as Control).Tag as string;
            ModelColumnsStates.ColumnState selectedMode = (ModelColumnsStates.ColumnState)int.Parse(btnTag[0].ToString());
            int selectedColumn = int.Parse(btnTag[1].ToString());

            ColumnModeManager.SetNewState(selectedColumn, selectedMode, true);

            UpdateVisibilityMenuItemsCheckedState();
        }

        private void InitializeVisibilityMenuItemsCheckedState()
        {
            ColumnModeManager = new ModelColumnsStates(new ColumnDefinition[] { ColumnItemTree, ColumnItemList, Column3DView },
                new double[] { 2f, 4f, 3f },
                new double[] { 350f, 450f, 0f },
                200f);

            bool[] columnVisibility = {
                this.userSettings.GetSettingBool(UserSettingEnum.MainColumnsVisibilityDirectoryTree),
                this.userSettings.GetSettingBool(UserSettingEnum.MainColumnsVisibilityModelList),
                this.userSettings.GetSettingBool(UserSettingEnum.MainColumnsVisibility3DView)
            };
            int poweredColumnIndex = this.userSettings.GetSettingInt(UserSettingEnum.MainColumnsPoweredIndex);

            for (int i = 0; i < columnVisibility.Length; i++)
            {
                if (!columnVisibility[i])
                    ColumnModeManager.SetNewState(i, ModelColumnsStates.ColumnState.Visibility, false);
                else if (poweredColumnIndex == i)
                    ColumnModeManager.SetNewState(i, ModelColumnsStates.ColumnState.Powered, false);
            }

            UpdateVisibilityMenuItemsCheckedState();
        }
        private void UpdateVisibilityMenuItemsCheckedState()
        {
            MenuColumnsShowDirectoryTree.IsChecked = ColumnModeManager.IsColumnEnabled(0);
            MenuColumnsShowModelList.IsChecked = ColumnModeManager.IsColumnEnabled(1);
            MenuColumnsShow3DView.IsChecked = ColumnModeManager.IsColumnEnabled(2);

            MenuColumnsPowerDirectoryTree.IsChecked = ColumnModeManager.IsColumnPowered(0);
            MenuColumnsPowerModelList.IsChecked = ColumnModeManager.IsColumnPowered(1);
            MenuColumnsPower3DView.IsChecked = ColumnModeManager.IsColumnPowered(2);

            ColumnGridSplitterLeft.Visibility = ColumnModeManager.IsColumnEnabled(0) && ColumnModeManager.IsColumnEnabled(1) ? Visibility.Visible : Visibility.Collapsed;
            ColumnGridSplitterRight.Visibility = ColumnModeManager.IsColumnEnabled(2) && ColumnModeManager.IsColumnEnabled(1) ? Visibility.Visible : Visibility.Collapsed;

            userSettings.SetSettingBool(UserSettingEnum.MainColumnsVisibilityDirectoryTree, ColumnModeManager.IsColumnEnabled(0));
            userSettings.SetSettingBool(UserSettingEnum.MainColumnsVisibilityModelList, ColumnModeManager.IsColumnEnabled(1));
            userSettings.SetSettingBool(UserSettingEnum.MainColumnsVisibility3DView, ColumnModeManager.IsColumnEnabled(2));
            userSettings.SetSettingInt(UserSettingEnum.MainColumnsPoweredIndex, ColumnModeManager.GetColumnPowered());
        }
    }

    #region Converters

    public class RadioBoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)value == (string)parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
    public class NullToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = value == null || (value is string && string.IsNullOrWhiteSpace((string)value));
            return (parameter == null ? result : !result) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    public class BoolToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(Visibility.Visible) ? true : false;
        }
    }
    public class BoolToVisibleHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(Visibility.Visible) ? true : false;
        }
    }
    public class StringToUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Uri(string.Format(parameter.ToString(), value.ToString()), UriKind.Relative);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }
    public class ItemsEqualsToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && parameter != null && value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    #endregion
}
