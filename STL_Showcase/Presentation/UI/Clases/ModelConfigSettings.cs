using STL_Showcase.Data.DataObjects;
using STL_Showcase.Logic.Localization;
using STL_Showcase.Shared.Enums;
using STL_Showcase.Shared.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STL_Showcase.Presentation.UI.Clases
{
    public class ModelConfigSettings : INotifyPropertyChanged
    {
        #region General settings

        public bool EnableDebugLogs { get; set; }
        public RenderAspectEnum SelectedThumbnailRenderAspec { get; set; }
        public bool UseGridIn3DView { get; set; }

        public bool EnableMeshDecimation { get; set; }
        public int MinTrianglesForMeshDecimation { get; set; }
        public bool EnableMaxSizeMBToLoadMeshInView { get; set; }
        public int MaxSizeMBToLoadMeshInView { get; set; }

        #endregion General settings

        #region Program settings

        public ObservableCollection<LinkedProgramData> LinkedProgramsData { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Program settings

        #region Advanced settings

        // TODO: Add advanced settings.

        #endregion Advanced settings

        public void LoadSettings()
        {
            var userSettings = DefaultFactory.GetDefaultUserSettings();

            LinkedProgramsData = new ObservableCollection<LinkedProgramData>(userSettings.GetSettingSerialized<List<LinkedProgramData>>(UserSettingEnum.ConfigLinkedProgramsList) ?? new List<LinkedProgramData>());
            EnableDebugLogs = userSettings.GetSettingBool(UserSettingEnum.EnableDebugLogs);
            SelectedThumbnailRenderAspec = (RenderAspectEnum)userSettings.GetSettingInt(UserSettingEnum.Thumbnails3DAspect);

            EnableMeshDecimation = userSettings.GetSettingBool(UserSettingEnum.EnableMeshDecimation);
            MinTrianglesForMeshDecimation = userSettings.GetSettingInt(UserSettingEnum.MinTrianglesForMeshDecimation);
            EnableMaxSizeMBToLoadMeshInView = userSettings.GetSettingBool(UserSettingEnum.EnableMaxSizeMBToLoadMeshInView);
            MaxSizeMBToLoadMeshInView = userSettings.GetSettingInt(UserSettingEnum.MaxSizeMBToLoadMeshInView);

            NotifyPropertyChanged(nameof(LinkedProgramsData));
            NotifyPropertyChanged(nameof(EnableDebugLogs));
            NotifyPropertyChanged(nameof(SelectedThumbnailRenderAspec));
            NotifyPropertyChanged(nameof(EnableMeshDecimation));
            NotifyPropertyChanged(nameof(MinTrianglesForMeshDecimation));
            NotifyPropertyChanged(nameof(EnableMaxSizeMBToLoadMeshInView));
            NotifyPropertyChanged(nameof(MaxSizeMBToLoadMeshInView));
        }

        public void SaveSettings()
        {
            var userSettings = DefaultFactory.GetDefaultUserSettings();

            userSettings.SetSettingSerialized<List<LinkedProgramData>>(UserSettingEnum.ConfigLinkedProgramsList, LinkedProgramsData.ToList());
            userSettings.SetSettingBool(UserSettingEnum.EnableDebugLogs, EnableDebugLogs);
            userSettings.SetSettingInt(UserSettingEnum.Thumbnails3DAspect, (int)SelectedThumbnailRenderAspec);

            userSettings.SetSettingBool(UserSettingEnum.EnableMeshDecimation, EnableMeshDecimation);
            userSettings.SetSettingInt(UserSettingEnum.MinTrianglesForMeshDecimation, MinTrianglesForMeshDecimation);
            userSettings.SetSettingBool(UserSettingEnum.EnableMaxSizeMBToLoadMeshInView, EnableMaxSizeMBToLoadMeshInView);
            userSettings.SetSettingInt(UserSettingEnum.MaxSizeMBToLoadMeshInView, MaxSizeMBToLoadMeshInView);
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
