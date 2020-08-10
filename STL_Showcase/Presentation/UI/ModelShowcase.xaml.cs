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
using STL_Showcase.Logic.Localization;
using STL_Showcase.Data.DataObjects;
using NLog;

namespace STL_Showcase.Presentation.UI
{

    /// <summary>
    /// Interaction logic for ModelShowcase.xaml
    /// </summary>
    public partial class ModelShowcase : System.Windows.Controls.Page
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
        private List<LinkedProgramData> _LinkedProgramsData { get; set; }

        IUserSettings userSettings = DefaultFactory.GetDefaultUserSettings();

        DateTime lastListFilterKey = DateTime.Now;
        CancellationTokenSource listFilterCancellationTokenSource;

        DirectoryLoadingAsync CurrentDirectoryLoader;

        public BitmapImage DefaultImageErrorModel { get; set; }
        private BitmapSource[] DefaultImageErrorModelArray;
        public BitmapImage DefaultImageLoading { get; set; }
        private BitmapSource[] DefaultImageLoadingArray;

        #endregion

        #region Initialization
        public ModelShowcase(MainWindow w)
        {
            InitializeComponent();

            userSettings = DefaultFactory.GetDefaultUserSettings();

            SetLoggingEnabled(userSettings.GetSettingBool(UserSettingEnum.EnableDebugLogs));

            InitializeVisibilityMenuItemsCheckedState();
            UpdateVisibilityMenuItemsCheckedState();

            _ModelItemListData = new ModelItemListData();
            _ModelItemListData.ZoomLevelChanged += (sender, e) => ModelListItem.SetImageSizeFor(_ModelItemListData.ModelListItemContentSize);
            _ModelItemListData.ZoomLevelChanged += ModelItemList_ZoomLevelChanged_UpdateScrollPosition;
            _ModelItemListData.ThumbnailScalingMode = userSettings.GetSettingBool(UserSettingEnum.EnableReduceThumbnailQuality) ? BitmapScalingMode.LowQuality : BitmapScalingMode.HighQuality;
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

            _LinkedProgramsData = userSettings.GetSettingSerialized<List<LinkedProgramData>>(UserSettingEnum.ConfigLinkedProgramsList);

            var viewport = new HelixViewport3D();
            view3d = new View3D(viewport);
            view3d.OptionRenderStyle = (RenderAspectEnum)userSettings.GetSettingInt(UserSettingEnum.CurrentView3DAspect);
            ViewportContainer.Children.Add(viewport);
            view3d.SetModel(null);
            view3d.UpdateLights();
            view3d.SetCameraRotationMode(userSettings.GetSettingBool(UserSettingEnum.EnableViewModelAutoRotation));

            _Model3DViewInfo = new Model3DViewInfo();
            View3DText.DataContext = _Model3DViewInfo;
            RenderTypeScrollItemContainer.DataContext = _Model3DViewInfo;
            ToggleButton3DAutoRotation.IsChecked = userSettings.GetSettingBool(UserSettingEnum.EnableViewModelAutoRotation);

            view3d.LoadModelInfoAvailableEvent += LoadModelInfoAvailable;

            this._w = w;
            _w.ClosingEvent += UnloadDirectories;
            InitializeResources();
            SetUILanguage();
            Loc.Ins.OnLanguageChanged += SetUILanguage;
            InitializeDirectoryLoading();
        }
        public void InitializeResources()
        {

            DefaultImageErrorModel = new BitmapImage(new Uri("pack://application:,,,/Presentation/UI/Styles/Resources/STL_Error.png", UriKind.Absolute));
            DefaultImageErrorModelArray = new[] { DefaultImageErrorModel };
            DefaultImageLoading = new BitmapImage(new Uri("pack://application:,,,/Presentation/UI/Styles/Resources/STL_Loading2.png", UriKind.Absolute));
            DefaultImageLoadingArray = new[] { DefaultImageLoading };

        }

        private void InitializeDirectoryLoading()
        {
            try
            {
                IEnumerable<string> lastDirectories = userSettings.GetSettingString(UserSettingEnum.LastLoadedDirectories).Split(';').Where(dir => !string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir));
                if (lastDirectories.Any())
                {
                    LoadDirectories(lastDirectories);
                }
            }
            catch
            {
                userSettings.SetSettingString(UserSettingEnum.LastLoadedDirectories, "");
            }
        }

        private void SetUILanguage(string newLanguage = "")
        {
            this.Title = Loc.GetText("AppName");
            tbAppTitle.Text = Loc.GetText("AppName");

            // Main menu
            {
                MenuColumnsLoadDirectory.Header = Loc.GetText("LoadDirectory");
                MenuColumnsLoadDirectory.ToolTip = Loc.GetText("tooltipLoadDirectory");
                MenuColumnsShowDirectoryTree.Header = Loc.GetText("View");

                MenuColumnsMain.Header = Loc.GetText("View");

                MenuColumnsShowDirectoryTree.Header = Loc.GetText("ShowDirectoryTree");
                MenuColumnsShowModelList.Header = Loc.GetText("ShowModelList");
                MenuColumnsShow3DView.Header = Loc.GetText("Show3DView");
                MenuColumnsPowerDirectoryTree.Header = Loc.GetText("ExpandDirectoryTree");
                MenuColumnsPowerModelList.Header = Loc.GetText("ExpandModelListView");
                MenuColumnsPower3DView.Header = Loc.GetText("Expand3DView");
                MenuColumnsResetVisibility.Header = Loc.GetText("ResetVisibility");
                MenuColumnsResetVisibility.ToolTip = Loc.GetText("tooltipResetVisibility");

                MenuItemConfiguration.Header = Loc.GetText("Configuration");

                MenuCacheMain.Header = Loc.GetText("Cache");
                MenuCacheAbout.Header = Loc.GetText("CacheWhatIsIt");
                MenuCacheReloadDisplayed.Header = Loc.GetText("ReloadCacheOfLoadedDirs");
                MenuCacheReloadDisplayed.ToolTip = Loc.GetText("tooltipReloadDisplayed");

                MenuAbout.Header = Loc.GetText("About");

            }

            // File tree panel
            {
                DirectoryUnloadAll.Content = Loc.GetText("UnloadAll");
                DirectoryUnloadAll.ToolTip = Loc.GetText("tooltipUnloadAll");
                DirectoryReloadAll.Content = Loc.GetText("ReloadAll");
                DirectoryReloadAll.ToolTip = Loc.GetText("tooltipReloadAll");
                TreeExpandAll.Content = Loc.GetText("ExpandAll");
                TreeExpandAll.ToolTip = Loc.GetText("tooltipExpandAll");
                TreeContractAll.Content = Loc.GetText("ContractAll");
                TreeContractAll.ToolTip = Loc.GetText("tooltipContractAll");


                btnFilterTypeOnlyFolders.Content = Loc.GetText("OnlyFolders");
                btnFilterTypeOnlyFolders.ToolTip = Loc.GetText("tooltipOnlyFolders");
                btnFilterTypeCollectionMode.Content = Loc.GetText("CollectionMode");
                btnFilterTypeCollectionMode.ToolTip = Loc.GetText("tooltipCollectionMode");
                tbFilesProcessed.Text = Loc.GetText("FilesProcessed:");
            }

            // Image list panel
            {
                ButtonReset3DCamera.ToolTip = Loc.GetText("tooltipButtonReset3DCamera");
                ToggleButton3DAutoRotation.ToolTip = Loc.GetText("tooltipEnableModelAutoRotation");
                tbFilter.Text = Loc.GetText("Filter");
                tbOrder.Text = Loc.GetText("Order");
                rbFilterNameModelList.Content = Loc.GetText("FileName");
                rbFilterDirectoryModelList.Content = Loc.GetText("Directory");
                rbFilterModifiedModelList.Content = Loc.GetText("DateModified");
                rbFilterCreatedModelList.Content = Loc.GetText("DateCreated");
                rbFilterSizeModelList.Content = Loc.GetText("Size");
            }

            // 3D Model View
            {
                btnUnset3DViewModel.Content = Loc.GetText("DeselectModelFrom3DView");
            }

            this.UpdateLayout();
        }

        #endregion

        #region Main Menu Events
        private void LoadDirectory_Click(object sender, RoutedEventArgs e)
        {
            LoadDirectoryWithDialog();
        }

        Task _CacheRelocationTask;
        private void MenuItemConfiguration_Click(object sender, RoutedEventArgs e)
        {
            ModelConfigSettings settingsOriginal = new ModelConfigSettings();
            settingsOriginal.LoadSettings();

            ConfigurationWindow config = new ConfigurationWindow();
            bool result = config.ShowDialog() ?? false;

            // Update stuff based on the changed settings
            if (result)
            {
                ModelConfigSettings settingsSettingsModified = new ModelConfigSettings();
                settingsSettingsModified.LoadSettings();

                _LinkedProgramsData = settingsSettingsModified.LinkedProgramsData.ToList();

                if (settingsSettingsModified.EnableDebugLogs != settingsOriginal.EnableDebugLogs)
                    SetLoggingEnabled(settingsSettingsModified.EnableDebugLogs);

                if ((_ModelItemListData.ThumbnailScalingMode == BitmapScalingMode.LowQuality) != settingsSettingsModified.EnableReduceThumbnailQuality)
                    _ModelItemListData.ThumbnailScalingMode = settingsSettingsModified.EnableReduceThumbnailQuality ? BitmapScalingMode.LowQuality : BitmapScalingMode.HighQuality;

                if (settingsOriginal.CachePath != settingsSettingsModified.CachePath)
                    SetThumnailCacheFolder(settingsOriginal.CachePath, settingsSettingsModified.CachePath);

                if (settingsOriginal.SelectedThumbnailRenderAspec != settingsSettingsModified.SelectedThumbnailRenderAspec ||
                settingsOriginal.EnableReduceThumbnailResolution != settingsSettingsModified.EnableReduceThumbnailResolution)
                    ReloadDirectories();
            }
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
                    MessageBox.Show(Loc.GetText("CacheFolderDontExistsYet"));
                else
                    Process.Start(cachePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Loc.GetTextFormatted("UnableOpenCacheFolder", cachePath));
            }
        }

        private async void MenuCacheDelete_Click(object sender, RoutedEventArgs e)
        {
            string deleteTitle = Loc.GetText("DeleteCacheFiles");
            string deleteQuestion = Loc.GetText("DeleteCacheFilesMessage");
            string deleteConfirmation = Loc.GetText("DeleteCacheFilesConfirmation");
            string deleteError = Loc.GetText("DeleteCacheFilesError");

            if ((await new MessageDialog(deleteQuestion, deleteTitle, Loc.GetText("Confirm"), "", Loc.GetText("Cancel")).ShowAsync()) == ContentDialogResult.Primary)
            {
                var cache = DefaultFactory.GetDefaultThumbnailCache();
                if (cache.ClearCache())
                {
                    await new MessageDialog(deleteConfirmation, deleteTitle, Loc.GetText("OK"), "", "").ShowAsync();
                }
                else
                {
                    await new MessageDialog(deleteError, deleteTitle, Loc.GetText("OK"), "", "").ShowAsync();
                }
            }
        }

        private async void MenuCacheAbout_Click(object sender, RoutedEventArgs e)
        {
            string message = Loc.GetText("AboutWindowMessage");
            ContentDialog dialog = new MessageDialog(message, Loc.GetText("AboutTheCache"), Loc.GetText("OK"), "", "");
            await dialog.ShowAsync();
        }

        private void MenuCacheReloadDisplayed_Click(object sender, RoutedEventArgs e)
        {
            var cache = DefaultFactory.GetDefaultThumbnailCache();
            cache.ClearCacheForFiles(_ModelItemListData.ModelList.Select(m => m.FileData.FileName));
            ReloadDirectories();
        }

        private async void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            string message = Loc.GetText("InformationAboutTheProgram");
            ContentDialog dialog = new MessageDialog(message, $"{Loc.GetText("About")} {Loc.GetText("AppName")}", Loc.GetText("OK"), "", "");
            await dialog.ShowAsync();
        }
        #endregion

        #region Model Tree Events

        private void ImageTreeControl_Item_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ModelTreeItem clickedItem = (ModelTreeItem)((Grid)sender).Tag;

            ContextMenu cm;
            if (clickedItem.HasData)
                cm = GetContextMenuForListItem(((ModelListItem)clickedItem.Data).FileData.FileFullPath, ((ModelListItem)clickedItem.Data).FileData.FileType);
            else
                cm = GetContextMenuForListItem(string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), clickedItem.GetTextsToRoot()), null);

            cm.PlacementTarget = sender as Button;
            cm.IsOpen = true;
        }

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
                    _ModelItemListData.ModelListDirectoryFilter = string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), newItem.GetTextsToRoot());
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
                    if (node.IsExpanded)
                        node.SetIsExpandedAll(true);
                    else
                        node.IsExpanded = true;
                    if (node.ParentItem != null) node.ParentItem.IsExpanded = true;
                }
        }

        private void TreeContractAll_Click(object sender, RoutedEventArgs e)
        {
            if (_ModelItemListData.ModelTreeRoot != null)
                foreach (var node in _ModelItemListData.ModelTreeRoot)
                {
                    bool hadAnyExpanded = node.ChildItems?.Any(ci => !ci.HasData && ci.IsExpanded) ?? false;
                    node.SetIsExpandedAll(false);
                    if (hadAnyExpanded)
                        node.IsExpanded = true;

                    if (node.ParentItem != null) node.ParentItem.IsExpanded = false;
                }
        }

        private void DirectoryUnloadAll_Click(object sender, RoutedEventArgs e)
        {
            UnloadDirectories();
        }

        private void DirectoryReloadAll_Click(object sender, RoutedEventArgs e)
        {
            ReloadDirectories();
        }

        private void DirectoryAddOneMore_Click(object sender, RoutedEventArgs e)
        {

        }
        private void FileTypeFilterToggleButton_Click(object sender, RoutedEventArgs e)
        {
            this._ModelItemListData.ApplyFilterToTree();
            userSettings.SetSettingBool(UserSettingEnum.EnableTreeOnlyFolders, this._ModelItemListData.FileOnlyFoldersFilter);
            ImageListControl.ItemsSource = this._ModelItemListData.ModelListFiltered;
        }
        private void btnFilterTypeCollectionMode_Click(object sender, RoutedEventArgs e)
        {
            userSettings.SetSettingBool(UserSettingEnum.EnableTreeCollections, this._ModelItemListData.FileCollectionMode);
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


        private void ImageListItemClick_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ModelListItem clickedItem = (ModelListItem)((Button)sender).CommandParameter;

            ContextMenu cm = GetContextMenuForListItem(clickedItem.FileData.FileFullPath, clickedItem.FileData.FileType);
            cm.PlacementTarget = sender as Button;
            cm.IsOpen = true;
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
                if (!_ModelItemListData.IsDirectoryLoaded(path, true))
                {
                    LoadDirectory(path);
                }
                else
                {
                    ContentDialog dialog = new MessageDialog(Loc.GetText("DirectoryAlreadyLoaded"), Loc.GetText("LoadDirectory"), Loc.GetText("OK"), "", "");
                    dialog.ShowAsync();
                }
            }
        }

        private void UnloadDirectory(string path)
        {
            if (CurrentDirectoryLoader != null && CurrentDirectoryLoader.IsLoading)
                CurrentDirectoryLoader.CancelOperation();

            _ModelItemListData.RemoveDirectoryLoaded(path);
            ReloadDirectories();
        }

        private void UnloadDirectories()
        {
            if (CurrentDirectoryLoader != null && CurrentDirectoryLoader.IsLoading)
                CurrentDirectoryLoader.CancelOperation();
            _ModelItemListData.Reset();
            ImageListControl.ItemsSource = _ModelItemListData.ModelListFiltered;
            ImageTreeControl.ItemsSource = _ModelItemListData.ModelTreeRoot;
            GC.Collect();
        }

        private void ReloadDirectories()
        {
            if (CurrentDirectoryLoader != null && CurrentDirectoryLoader.IsLoading)
                CurrentDirectoryLoader.CancelOperation();

            var loadedDirectories = _ModelItemListData.GetDirectoriesLoaded();
            _ModelItemListData.Reset();
            ImageListControl.ItemsSource = _ModelItemListData.ModelListFiltered;
            ImageTreeControl.ItemsSource = _ModelItemListData.ModelTreeRoot;

            LoadDirectories(loadedDirectories);



            GC.Collect();
        }

        private void LoadDirectoryFileReady(ModelFileData modelFile, BitmapSource[] cacheImages, LoadResultEnum result)
        {
            try
            {
                if (CurrentDirectoryLoader != null && CurrentDirectoryLoader.IsLoading)
                    _ = this.Dispatcher.BeginInvoke(() =>
                      {
                          ModelListItem listItem = this._ModelItemListData.GetFromInternalDictionary(modelFile.FileFullPath);
                          if (listItem != null)
                          {
                              BitmapSource[] images = cacheImages;
                              if (result != LoadResultEnum.Okay)
                              {
                                  images = DefaultImageErrorModelArray;
                                  listItem.ErrorMessage = Loc.GetText(result.ToString());
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

        private void LoadDirectory(string directories)
        {
            LoadDirectories(new string[] { directories });
        }

        private void LoadDirectories(IEnumerable<string> directories)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            IEnumerable<string> loadedDirectories = _ModelItemListData.GetDirectoriesLoaded().Concat(directories);

            if (!loadedDirectories.Any())
            {
                new MessageDialog(Loc.GetText("NoDirectoriesSelected"), Loc.GetText("LoadDirectory"), Loc.GetText("OK"), "", "").ShowAsync();
                return;
            }

            LoadingDialog loading = new LoadingDialog(Loc.GetTextFormatted("LookingForFilesAtDir", string.Join("\", \"", directories)), Loc.GetText("LoadDirectory"), Loc.GetText("Cancel"), () => source.Cancel(false));
            Task loadingTask = loading.ShowAsync();

            if (CurrentDirectoryLoader != null && CurrentDirectoryLoader.IsLoading)
            {
                // Cancel current loading, if any
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

            Task.Factory.StartNew(async () =>
            {
                CancellationToken t = source.Token;
                try
                {
                    if (CurrentDirectoryLoader.LoadDirectories(loadedDirectories))
                    {
                        if (t.IsCancellationRequested)
                            return;

                        this.Dispatcher.Invoke(() =>
                        {
                            UnloadDirectories(); return true;
                        });

                        Tuple<ModelFileData, LoadResultEnum>[] modelFiles = CurrentDirectoryLoader.FilesFound.Select(m => new Tuple<ModelFileData, LoadResultEnum>(m, LoadResultEnum.Okay)).OrderBy(f => f.Item2).ToArray();
                        // List View
                        {
                            BitmapScalingMode thumnailScalingMode = userSettings.GetSettingBool(UserSettingEnum.EnableReduceThumbnailQuality) ? BitmapScalingMode.LowQuality : BitmapScalingMode.HighQuality;
                            List<ModelListItem> newModelList = new List<ModelListItem>(modelFiles.Length);
                            foreach (Tuple<ModelFileData, LoadResultEnum> fileData in modelFiles)
                            {
                                var modelListItem = new ModelListItem();
                                modelListItem.FileData = fileData.Item1;
                                modelListItem.ScalingMode = thumnailScalingMode;
                                modelListItem.SetImages(fileData.Item2 == LoadResultEnum.Okay ? DefaultImageLoadingArray : DefaultImageErrorModelArray);
                                modelListItem.ErrorMessage = fileData.Item2 == LoadResultEnum.Okay ? string.Empty : Loc.GetText(fileData.Item2.ToString());
                                newModelList.Add(modelListItem);
                            }
                            this.Dispatcher.Invoke(() =>
                            {
                                _ModelItemListData.ModelList = new ObservableCollection<ModelListItem>(newModelList);
                                return true;
                            });
                        }

                        // Tree View
                        {
                            IEnumerable<Tuple<string[], object>> tuples = _ModelItemListData.ModelList.Select(m => new Tuple<string[], object>((m.FileData.FileFullPath).Split(System.IO.Path.DirectorySeparatorChar), m));
                            //IEnumerable<Tuple<string[], object>> tuples = _ModelItemListData.ModelList.OrderBy(m => m.FileData.FileName).OrderBy(m => m.FileData.FilePath).Select(m => new Tuple<string[], object>((m.FileData.FileFullPath).Split(System.IO.Path.DirectorySeparatorChar), m));

                            IEnumerable<string> treeRootsStrings = tuples.Select(tp => tp.Item1[0]).Distinct();
                            ModelTreeItem[] treeRoots = treeRootsStrings.Select((item1) =>
                            {
                                return new ModelTreeItem(1, item1, null, (mfd) =>
                                {
                                    return ((ModelListItem)mfd).ImageSmallest;
                                });
                            }).ToArray();

                            ObservableCollection<ModelTreeItem> trimmedTreeRoots = new ObservableCollection<ModelTreeItem>();
                            foreach (var treeRoot in treeRoots)
                            {
                                treeRoot.BuildTreeRecursive(tuples);
                                var trimmedTree = treeRoot.Trim();

                                foreach (var node in trimmedTree)
                                {
                                    if (!node.HasExpandedCache)
                                        node.IsExpanded = true;
                                }
                                trimmedTreeRoots = new ObservableCollection<ModelTreeItem>(trimmedTreeRoots.Concat(trimmedTree).OrderBy(tr => tr.Text));
                            }

                            // Order and generate collections.
                            foreach (var trimmedTreeRoot in trimmedTreeRoots)
                            {
                                if (userSettings.GetSettingBool(UserSettingEnum.EnableTreeCollections))
                                    trimmedTreeRoot.GenerateCollectionsRecursive();
                                trimmedTreeRoot.OrderChildsByDataAndText(true);
                            }


                            this.Dispatcher.Invoke(() => { _ModelItemListData.ModelTreeRoot = trimmedTreeRoots; return true; });
                        }

                        // Save directories loaded
                        await this.Dispatcher.Invoke(async () =>
                        {
                            string[] rootLoadedDirectories = _ModelItemListData.ModelTreeRoot.Select(tr =>
                            {
                                string[] rootPathAsArray = tr.GetTextsToRoot().ToArray();
                                return System.IO.Path.Combine(string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), rootPathAsArray));
                            }).ToArray();

                            _ModelItemListData.AddDirectoriesLoaded(rootLoadedDirectories); // loadedDirectories
                            userSettings.SetSettingString(UserSettingEnum.LastLoadedDirectories, string.Join(";", rootLoadedDirectories)); // loadedDirectories
                        });

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
                                new MessageDialog(Loc.GetText("LoadDirectoryUnable"), Loc.GetText("LoadingError"), Loc.GetText("OK"), "", "").ShowAsync();
                            else
                                new MessageDialog(Loc.GetTextFormatted("LoadDirectoryNoFilesAtDir", string.Join("\", \"", directories)), Loc.GetText("NothingFound!"), Loc.GetText("OK"), "", "").ShowAsync();
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
                    this.Dispatcher.Invoke(() => { UnloadDirectories(); return true; });
                }
                catch (Exception ex)
                {
                    mainLogger.Trace(ex, "An unexpected error happened when loading directories.");
                }
                finally
                {
                    GC.Collect();
                    this.Dispatcher.Invoke(() => loading.CloseDialog());
                }
            }, source.Token);

        }

        #endregion

        #region Model Viewport

        private void LoadModelInViewport(ModelFileData modelData)
        {
            this.view3d.SetModel(modelData);
            this.view3d.Viewport.UpdateLayout();
        }

        private void LoadModelInfoAvailable(string name, int tris, int verts, int sizeKB)
        {
            this._Model3DViewInfo.SetData(name, tris, verts, sizeKB);

        }
        private void ButtonReset3DCamera_Click(object sender, RoutedEventArgs e)
        {
            view3d.ResetCamera(View3D.CameraPositionEnum.Default);
        }

        private void ButtonTake3DScreenShot_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ToggleButton3DAutoRotation_Click(object sender, RoutedEventArgs e)
        {
            bool newState = ((ToggleButton)sender).IsChecked ?? true;
            userSettings.SetSettingBool(UserSettingEnum.EnableViewModelAutoRotation, newState);
            view3d.SetCameraRotationMode(newState);
        }

        private void btnUnset3DViewModel_Click(object sender, RoutedEventArgs e)
        {
            LoadModelInViewport(null);
            _ModelItemListData.SelectedListItem = null;
        }

        #endregion


        private void SetLoggingEnabled(bool enabled)
        {
            foreach (var rule in LogManager.Configuration.LoggingRules)
            {
                if (enabled)
                {
                    foreach (var target in rule.Targets)
                    {
                        LogLevel minLevel;
                        switch (target.Name)
                        {
                            case "AllFileLog":
                            case "Loader":
                            case "Parser":
                                minLevel = LogLevel.Trace; break;
                            case "GeneralLogFile":
                                minLevel = LogLevel.Debug; break;
                            default:
                                minLevel = LogLevel.Info; break;
                        }
                        rule.SetLoggingLevels(minLevel, LogLevel.Fatal);

                        break;
                    }
                }
                else
                {
                    rule.SetLoggingLevels(LogLevel.Off, LogLevel.Off);
                }
            }

            LogManager.ReconfigExistingLoggers();
        }

        private void SetThumnailCacheFolder(CacheEnums.CachePathType oldPath, CacheEnums.CachePathType newPath)
        {
            if (_CacheRelocationTask == null)
            {
                var cacheInstance = DefaultFactory.GetDefaultThumbnailCache();
                LoadingDialog loading = new LoadingDialog(Loc.GetTextFormatted(CurrentDirectoryLoader != null && CurrentDirectoryLoader.IsLoading ? "MovingCacheFolderWaiting" : "MovingCacheFolder", cacheInstance.GetCurrentCachePath()), Loc.GetText("CacheFolderSetting"));
                loading.ShowAsync();

                _CacheRelocationTask = Task.Run(async () =>
                {
                    while (CurrentDirectoryLoader != null && CurrentDirectoryLoader.IsLoading)
                        await Task.Delay(500);

                    await Task.Delay(1000);

                    await Dispatcher.Invoke(async () =>
                    {
                        bool cacheMoved = cacheInstance.MoveCacheToNewLocation(oldPath, newPath);
                        loading.Hide();

                        if (!cacheMoved)
                            await new MessageDialog(Loc.GetTextFormatted("ErrorMovingCacheFolder", cacheInstance.GetCurrentCachePath()), Loc.GetText("CacheFolderSetting"), Loc.GetText("OK"), "", "").ShowAsync();
                        else
                            await new MessageDialog(Loc.GetTextFormatted("CacheMovedToNewLocation", cacheInstance.GetCurrentCachePath()), Loc.GetText("CacheFolderSetting"), Loc.GetText("OK"), "", "").ShowAsync();


                        _CacheRelocationTask = null;
                    });

                });
            }
        }

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

        public void OpenFileWithProgram(string filePath, string program)
        {
            var pi = new ProcessStartInfo(filePath)
            {
                Arguments = System.IO.Path.GetFileName(filePath),
                UseShellExecute = true,
                WorkingDirectory = System.IO.Path.GetDirectoryName(filePath),
                FileName = program, // .exe
                Verb = "OPEN"
            };
            Process.Start(pi);

            // TODO: Show that the file is opening.
        }

        public ContextMenu GetContextMenuForListItem(string fullPathToItem, Supported3DFiles? fileType)
        {
            ContextMenu menu = new ContextMenu();

            // Open file/directory
            if (fileType.HasValue)
            {
                MenuItem menuItemOpenDefault = new MenuItem() { Header = Loc.GetText("OpenFile") };
                menuItemOpenDefault.Click += (sender, e) => Process.Start(fullPathToItem);
                menu.Items.Add(menuItemOpenDefault);
            }


            // View file in directory
            MenuItem menuItemOpenInDir = new MenuItem() { Header = Loc.GetText("ViewFileInExplorer") };
            menuItemOpenInDir.Click += (sender, e) =>
            {
                if (fileType.HasValue)
                    Process.Start("explorer.exe", $"/select, \"{fullPathToItem}\"");
                else
                    Process.Start(fullPathToItem);
            };
            menu.Items.Add(menuItemOpenInDir);

            // Unload directory
            if (!fileType.HasValue && _ModelItemListData.GetDirectoriesLoaded().Contains(fullPathToItem))
            {
                MenuItem menuItemUnloadDir = new MenuItem() { Header = Loc.GetText("UnloadDirectory") };
                menuItemUnloadDir.Click += (sender, e) => UnloadDirectory(fullPathToItem);
                menu.Items.Add(menuItemUnloadDir);
            }

            // Open file with software
            if (_LinkedProgramsData.Any())
            {

                List<MenuItem> ItemsForOpenWith = new List<MenuItem>();

                foreach (var data in _LinkedProgramsData.Where(lpd =>
                 (lpd.SupportSTL && fileType == Supported3DFiles.STL_ASCII || fileType == Supported3DFiles.STL_Binary) ||
                 (lpd.SupportOBJ && fileType == Supported3DFiles.OBJ) ||
                 (lpd.Support3MF && fileType == Supported3DFiles._3MF) ||
                 (lpd.SupportDirectory && !fileType.HasValue)))
                {
                    MenuItem menuItemForFile = new MenuItem() { Header = Loc.GetTextFormatted("OpenFileWith", data.ProgramName) };
                    menuItemForFile.Click += (sender, e) => OpenFileWithProgram(fullPathToItem, data.ProgramFullPath);
                    ItemsForOpenWith.Add(menuItemForFile);
                }

                if (ItemsForOpenWith.Any())
                {
                    menu.Items.Add(new Separator());
                    menu.Items.Add(ItemsForOpenWith);
                }
            }

            menu.Items.Add(new Separator());

            // Cancel
            MenuItem menuItemCancel = new MenuItem() { Header = Loc.GetText("Cancel") };
            menuItemCancel.Click += (sender, e) => menu.IsOpen = false;
            menu.Items.Add(menuItemCancel);


            return menu;
        }

        private void btnPerspective_Click(object sender, RoutedEventArgs e)
        {
            string tag = ((Button)sender).Tag.ToString();
            bool altDir = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.LeftShift);
            SetSpecialCameraDirection(tag, altDir);

        }

        private void btnPerspective_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            string tag = ((Button)sender).Tag.ToString();
            SetSpecialCameraDirection(tag, true);
        }

        private void SetSpecialCameraDirection(string buttonTag, bool altDir)
        {
            View3D.CameraPositionEnum cameraDirection = View3D.CameraPositionEnum.Current;

            switch (buttonTag)
            {
                case "Up":
                    cameraDirection = View3D.CameraPositionEnum.Up; break;
                case "Front":
                    cameraDirection = altDir ? View3D.CameraPositionEnum.Back : View3D.CameraPositionEnum.Front; break;
                case "Side":
                    cameraDirection = altDir ? View3D.CameraPositionEnum.OtherSide : View3D.CameraPositionEnum.Side; break;
            }

            view3d.ResetCamera(cameraDirection);
        }
    }

    #region Converters

    public class RadioBoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value).Equals((string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
    public class RenderAspectIndexMatchToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)((RenderAspectEnum)value) == ((int)parameter) ? "True" : "False";
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

    public class StringKeyToLocalizedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string translated = Loc.GetText(parameter == null ? (string)value : (string)parameter);
            return parameter != null ? string.Format(translated, value) : translated;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
    #endregion
}
