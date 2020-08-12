using IWshRuntimeLibrary;
using Ookii.Dialogs.Wpf;
using STL_Showcase.Logic.Localization;
using STL_Showcase.Presentation.UI.Clases;
using STL_Showcase.Presentation.UI.Clases.Utility;
using STL_Showcase.Shared.Enums;
using STL_Showcase.Shared.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using System.Reflection;

namespace STL_Showcase.Presentation.UI
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {

        static NLog.Logger logger = NLog.LogManager.GetLogger("Settings");

        #region Initialization

        public ModelConfigSettings _modelConfigSettingsOriginal { get; set; }
        public ModelConfigSettings _modelConfigSettings { get; set; }
        public Model3DViewInfo _model3DViewInfo { get; set; }

        public ObservableCollection<ComboboxItemToString> SupportedLanguages { get; set; }

        public ObservableCollection<ComboboxItemToString> CacheFolders { get; set; }

        public class ComboboxItemToString
        {
            public string ID { get; set; }
            public string DisplayName { get; set; }
            public override string ToString()
            {
                return DisplayName;
            }
        }

        public ConfigurationWindow()
        {
            InitializeComponent();

            _model3DViewInfo = new Model3DViewInfo();
            cbLanguage.DataContext = this;
            cbCacheFolder.DataContext = this;

            logger.Info("Opened settings window");

            Loc.Ins.OnLanguageChanged += SetUILanguage;
            SetUILanguage();
            InitializeSettingsFields();
        }

        private void SetUILanguage(string newLanguage = "")
        {
            this.Title = $"{Loc.GetText("AppName")} {Loc.GetText("Settings")}";
            tbWindowTitle.Text = Loc.GetText("Configuration");
            {
                GeneralSettings.Header = Loc.GetText("GeneralSettingsName");
                tbLanguageSetting.Text = Loc.GetText("Language");
                tbCacheFolder.Text = Loc.GetText("CacheFolderSetting");
                tbCacheFolder.ToolTip = Loc.GetText("tooltipCacheFolderSetting");
                tbEnableDebugLogs.Text = Loc.GetText("EnableDebugLogs");
                btnOpenLogsFolder.Content = $"   {Loc.GetText("OpenLogsFolder")}   ";
                tbThumnailStyle.Text = Loc.GetText("ThumbnailStyle");
                chkEnableThumnailColorsByShaders.Content = Loc.GetText("EnableThumnailColorsByShaders");
                chkEnableThumnailColorsByShaders.ToolTip = Loc.GetText("tooltipEnableThumnailColorsByShaders");
                chkEnableChangingViewColorChangesThumnailColor.Content = Loc.GetText("EnableChangingViewChangesThumnails");
                chkEnableChangingViewColorChangesThumnailColor.ToolTip = Loc.GetText("tooltipEnableChangingViewChangesThumnails");
            }

            {
                ProgramsSettings.Header = Loc.GetText("ProgramsSettingsName");
                tbProgramsGridTitle.Text = Loc.GetText("ProgramsTable");
                tbProgramsDescription.Text = Loc.GetText("ProgramsSettingsDescription");
                btnAddNewProgram.Content = Loc.GetText("AddNew");
            }

            {
                AdvancedSettings.Header = Loc.GetText("AdvancedSettingsName");

                tbPerformance.Text = Loc.GetText("PerformanceSettings");

                chkEnableMeshDecimate.Content = Loc.GetText("EnableMeshDecimate");
                nmbMeshDecimateMinTris.Description = Loc.GetText("MeshDecimateMinTris");
                chkEnableMeshDecimate.ToolTip = nmbMeshDecimateMinTris.ToolTip = Loc.GetText("tooltipEnableMeshDecimate");

                chkEnableMaxSizeModel.Content = Loc.GetText("EnableMaxSizeModel");
                nmbMaxSizeModelToView.Description = Loc.GetText("MaxSizeModelToView");
                nmbMaxSizeModelToView.ToolTip = chkEnableMaxSizeModel.ToolTip = Loc.GetText("tooltipEnableMaxSizeModel");

                chkEnableReduceThumbnailResolution.Content = Loc.GetText("GenerateThumnailLowRes");
                chkEnableReduceThumbnailResolution.ToolTip = Loc.GetText("tooltipGenerateThumnailLowRes");
                chkReduceEnableReduceThumbnailQuality.Content = Loc.GetText("UseFastThumbnailDrawing");
                chkReduceEnableReduceThumbnailQuality.ToolTip = Loc.GetText("tooltipUseFastThumbnailDrawing");
            }

            btnAutoretectPrograms.Content = Loc.GetText("AutoDetect3DSoftwareButton");
            btnAccept.Content = Loc.GetText("Accept");
            btnCancel.Content = Loc.GetText("Cancel");
        }

        private void InitializeSettingsFields()
        {
            _modelConfigSettings = new ModelConfigSettings();
            _modelConfigSettings.LoadSettings();

            #region General Settings
            {
                IEnumerable<CultureInfo> supportedCultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures).Where(ci => Loc.Ins.LoadedLanguages.Contains(ci.TwoLetterISOLanguageName));
                SupportedLanguages = new ObservableCollection<ComboboxItemToString>(supportedCultures
                    .Select(ci => new ComboboxItemToString() { ID = ci.TwoLetterISOLanguageName, DisplayName = ci.NativeName.First().ToString().ToUpper() + ci.NativeName.Substring(1) })
                    .OrderBy(cbi => cbi.DisplayName));

                cbLanguage.SelectedItem = SupportedLanguages.FirstOrDefault(cbi => cbi.ID == Loc.Ins.CurrentLanguage);

                IEnumerable<CacheEnums.CachePathType> cachePathTypes = (CacheEnums.CachePathType[])Enum.GetValues(typeof(CacheEnums.CachePathType));
                CacheFolders = new ObservableCollection<ComboboxItemToString>(cachePathTypes
                    .Select(n => new ComboboxItemToString() { ID = ((int)n).ToString(), DisplayName = Loc.GetText(n.ToString()) })
                    .OrderBy(n => n.DisplayName));

                cbCacheFolder.SelectedItem = CacheFolders.FirstOrDefault(cbi => cbi.ID == ((int)_modelConfigSettings.CachePath).ToString());


                chkEnableDebugLogs.DataContext = _modelConfigSettings;

                RenderTypeScrollItemContainer.DataContext = _model3DViewInfo;
                RenderTypeScrollItemContainer.UpdateLayout();

                chkEnableThumnailColorsByShaders.DataContext = _modelConfigSettings;
                chkEnableChangingViewColorChangesThumnailColor.DataContext = _modelConfigSettings;
                // TODO: Set the checked item, somehow.
                //RenderTypeScrollItemContainer.ItemContainerGenerator.StatusChanged += (sender, e) =>
                //{
                //    if (RenderTypeScrollItemContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                //    {
                //        ContentPresenter cp = RenderTypeScrollItemContainer.ItemContainerGenerator.ContainerFromIndex(0) as ContentPresenter;
                //    }
                //};
            }
            #endregion General Settings

            #region Linked Programs Settings
            {
                dgPrograms.DataContext = _modelConfigSettings;
            }
            #endregion Linked Programs Settings

            #region Advanced Settings
            {
                chkEnableMeshDecimate.DataContext = _modelConfigSettings;
                nmbMeshDecimateMinTris.DataContext = _modelConfigSettings;
                chkEnableMaxSizeModel.DataContext = _modelConfigSettings;
                nmbMaxSizeModelToView.DataContext = _modelConfigSettings;

                chkEnableReduceThumbnailResolution.DataContext = _modelConfigSettings;
                chkReduceEnableReduceThumbnailQuality.DataContext = _modelConfigSettings;
            }
            #endregion Advanced Settings
        }

        #endregion Initialization

        #region UI Events

        private void Window_Closed(object sender, EventArgs e)
        {
            logger.Info("Settings window: Closed");
            Loc.Ins.OnLanguageChanged -= SetUILanguage;
        }

        private void RenderTypeScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            RenderTypeScrollViewer.ScrollToHorizontalOffset(RenderTypeScrollViewer.HorizontalOffset + e.Delta);
        }

        private void RenderTypeButton_Click(object sender, RoutedEventArgs e)
        {
            int renderAspectInt = int.Parse(((RadioButton)sender).Tag.ToString());
            _modelConfigSettings.SelectedThumbnailRenderAspec = (RenderAspectEnum)renderAspectInt;
        }

        private void dgPrograms_Selected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(DataGridCell))
                ((DataGrid)sender).BeginEdit(e);
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("Settings window: Accept");
            _modelConfigSettings.CachePath = (CacheEnums.CachePathType)(int.TryParse(((ComboboxItemToString)cbCacheFolder.SelectedItem).ID, out int newCachePath) ? newCachePath : 0);
            _modelConfigSettings.SaveSettings();
            Loc.Ins.SetLanguage(((ComboboxItemToString)cbLanguage.SelectedItem).ID);
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("Settings window: Cancel");
            this.DialogResult = false;
            this.Close();
        }

        private void btnAddNewProgram_Click(object sender, RoutedEventArgs e)
        {
            ChooseAppWithDialog();
        }

        private void btnAutoretectPrograms_Click(object sender, RoutedEventArgs e)
        {
            AutoDetectProgramsAsync();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.GetPosition(this).Y < 30 && e.ClickCount == 1)
                this.DragMove();
        }

        #endregion UI Events


        #region Methods

        private void ChooseAppWithDialog()
        {
            VistaOpenFileDialog fileDialog = new VistaOpenFileDialog() { CheckFileExists = true, AddExtension = true };
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            fileDialog.Filter = "exe files (*.exe)|*.exe";
            fileDialog.Multiselect = false;

            var result = fileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                string path = fileDialog.FileName;

                _modelConfigSettings.LinkedProgramsData.Add(new Data.DataObjects.LinkedProgramData()
                {
                    ProgramName = System.IO.Path.GetFileNameWithoutExtension(path),
                    ProgramFullPath = path,
                    SupportSTL = true,
                });
            }
        }

        private async Task<IEnumerable<string>> AutoFillProgramsTableAsync()
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetAssembly(typeof(ConfigurationWindow)).Location);
            string programsListFile = System.IO.Path.Combine(appPath, "Content", "Autodetect3DSoftwareList.txt");

            if (!System.IO.File.Exists(programsListFile))
            {
                return new string[0];
            }

            string[] recognicedProgramsNames = System.IO.File.ReadAllLines(programsListFile);

            string programsFolder1 = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            string programsFolder2 = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string programsFolder3 = Environment.GetFolderPath(Environment.SpecialFolder.Favorites);
            string programsFolder4 = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\\Internet Explorer\\Quick Launch\\User Pinned\\TaskBar");
            string[] allProgramsFolder = { programsFolder1, programsFolder2, programsFolder3, programsFolder4 };

            WshShell shell = new WshShell();
            List<string> addedPrograms = new List<string>();

            foreach (string programFolder in allProgramsFolder)
            {
                try
                {
                    var allShortcutsFound = UtilMethods.EnumerateFiles(programFolder, "*.lnk", SearchOption.AllDirectories);

                    foreach (string shortcutFound in allShortcutsFound)
                    {
                        try
                        {
                            IWshShortcut link = (IWshShortcut)shell.CreateShortcut(shortcutFound);

                            string exeFileName = System.IO.Path.GetFileNameWithoutExtension(link.TargetPath);
                            if (recognicedProgramsNames.Any(p => string.Equals(p, exeFileName, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                if (_modelConfigSettings.LinkedProgramsData.Any(p => p.ProgramFullPath == link.TargetPath))
                                    continue;

                                var newRow = new Data.DataObjects.LinkedProgramData()
                                {
                                    ProgramName = System.IO.Path.GetFileNameWithoutExtension(shortcutFound),
                                    ProgramFullPath = link.TargetPath,
                                    SupportSTL = true,
                                };

                                await this.Dispatcher.BeginInvoke(() =>
                               {
                                   _modelConfigSettings.LinkedProgramsData.Add(newRow);
                               });

                                addedPrograms.Add(newRow.ProgramName);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Debug(ex, "Settings window: AutoFillProgramsTableAsync: Exception when processing shortcut '{0}'", shortcutFound);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(ex, "Settings window: AutoFillProgramsTableAsync: Exception when processing programs folder '{0}'", programFolder);
                }
            }
            return addedPrograms;
        }

        #endregion

        private async void AutoDetectProgramsAsync()
        {
            logger.Info("Settings window: AutoDetectProgramsAsync");

            LoadingDialog loading = new LoadingDialog(Loc.GetText("LookingFor3dSoftwareShortcuts"), Loc.GetText("AutoDetect3DSoftware"));

            loading.ShowAsync();

            Task<IEnumerable<string>> autoAddTask = Task.Run(AutoFillProgramsTableAsync);
            await autoAddTask;

            loading.CloseDialog();

            if (autoAddTask.Result.Any())
            {
                logger.Info($"Settings window: Autodetected {autoAddTask.Result.Count()} programs: [{string.Join("] [", autoAddTask.Result)}]");

                await new MessageDialog(Loc.GetTextFormatted("AutoDetect3DSoftware_FoundList", autoAddTask.Result.Count(), string.Join("\n", autoAddTask.Result)),
                    Loc.GetText("AutoDetect3DSoftware"), Loc.GetText("OK"), "", "").ShowAsync();
            }
            else
            {
                logger.Info("Settings window: Autodetected 0 programs.");
                await new MessageDialog(Loc.GetText("AutoDetect3DSoftware_NothingFound"), Loc.GetText("AutoDetect3DSoftware"), Loc.GetText("OK"), "", "").ShowAsync();
            }
        }

        private void btnOpenLogsFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string appPath = System.IO.Path.GetDirectoryName(Assembly.GetAssembly(typeof(ConfigurationWindow)).Location);
                string logsPath = System.IO.Path.Combine(appPath, "Logs");

                if (Directory.Exists(logsPath))
                {
                    System.Diagnostics.Process.Start(logsPath);
                }
                else
                {
                    new MessageDialog(Loc.GetText("LogsFolderDontExists"), Loc.GetText("OpenLogsFolder"), Loc.GetText("OK"), "", "").ShowAsync();
                }
            }
            catch (Exception ex)
            {
                new MessageDialog(Loc.GetText("ErrorOpeningLogsFolder"), Loc.GetText("OpenLogsFolder"), Loc.GetText("OK"), "", "").ShowAsync();
                logger.Trace(ex, "Error when trying to open logs folder (yet here you are, reading a log file).");
            }
        }
    }
}
