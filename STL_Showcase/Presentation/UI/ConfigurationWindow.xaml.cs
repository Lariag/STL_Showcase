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
        #region Initialization

        public ModelConfigSettings _modelConfigSettingsOriginal { get; set; }
        public ModelConfigSettings _modelConfigSettings { get; set; }
        public Model3DViewInfo _model3DViewInfo { get; set; }

        public ObservableCollection<ComboboxItemToString> SupportedLanguages { get; set; }

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
                tbEnableDebugLogs.Text = Loc.GetText("EnableDebugLogs");
                tbThumnailStyle.Text = Loc.GetText("ThumbnailStyle");
            }

            {
                ProgramsSettings.Header = Loc.GetText("ProgramsSettingsName");
                tbProgramsGridTitle.Text = Loc.GetText("ProgramsTable");
                tbProgramsDescription.Text = Loc.GetText("ProgramsSettingsDescription");
                btnAddNewProgram.Content = Loc.GetText("AddNew");
            }

            {
                AdvancedSettings.Header = Loc.GetText("AdvancedSettingsName");
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

                chkEnableDebugLogs.DataContext = _modelConfigSettings;

                RenderTypeScrollItemContainer.DataContext = _model3DViewInfo;
                RenderTypeScrollItemContainer.UpdateLayout();
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

            }
            #endregion Advanced Settings
        }

        #endregion Initialization

        #region UI Events

        private void Window_Closed(object sender, EventArgs e)
        {
            Loc.Ins.OnLanguageChanged -= SetUILanguage;
        }

        private void btnSelectProgramPath_Click(object sender, RoutedEventArgs e)
        {

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
            _modelConfigSettings.SaveSettings();
            Loc.Ins.SetLanguage(((ComboboxItemToString)cbLanguage.SelectedItem).ID);
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnAddNewProgram_Click(object sender, RoutedEventArgs e)
        {
            ChooseAppWithDialog();
        }

        private void btnAutoretectPrograms_Click(object sender, RoutedEventArgs e)
        {
            AutoretectProgramsAsync();
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
            string programsListFile = System.IO.Path.Combine(appPath, "Autodetect3DSoftwareList.txt");

            if (!System.IO.File.Exists(programsListFile))
            {
                return new string[0];
            }

            string[] recognicedProgramsNames = System.IO.File.ReadAllLines(programsListFile);

            string programsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            WshShell shell = new WshShell();
            List<string> addedPrograms = new List<string>();

            foreach (string shortcutFound in UtilMethods.EnumerateFiles(programsFolder, "*.lnk", SearchOption.AllDirectories))
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

            return addedPrograms;
        }

        #endregion

        private async void AutoretectProgramsAsync()
        {
            LoadingDialog loading = new LoadingDialog(Loc.GetText("LookingFor3dSoftwareShortcuts"), string.Empty, Loc.GetText("AutoDetect3DSoftware"));

            loading.ShowAsync();

            Task<IEnumerable<string>> autoAddTask = Task.Run(AutoFillProgramsTableAsync);
            await autoAddTask;

            loading.CloseDialog();

            if (autoAddTask.Result.Any())
            {
                await new MessageDialog(string.Format(Loc.GetText("AutoDetect3DSoftware_FoundList"), autoAddTask.Result.Count(), string.Join("\n", autoAddTask.Result)),
                    Loc.GetText("AutoDetect3DSoftware"), Loc.GetText("OK"), "", "").ShowAsync();
            }
            else
            {
                await new MessageDialog(Loc.GetText("AutoDetect3DSoftware_NothingFound"), Loc.GetText("AutoDetect3DSoftware"), Loc.GetText("OK"), "", "").ShowAsync();
            }
        }
    }
}
