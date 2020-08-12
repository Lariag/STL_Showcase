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

        public bool EnableThumnailColorsByShaders { get; set; }

        public bool EnableChangingViewColorChangesThumnailColor { get; set; }

        public CacheEnums.CachePathType CachePath { get; set; }

        #endregion General settings

        #region Program settings

        public ObservableCollection<LinkedProgramData> LinkedProgramsData { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Program settings

        #region Advanced settings

        public bool EnableMeshDecimation { get; set; }
        public int MinTrianglesForMeshDecimation { get; set; }
        public bool EnableMaxSizeMBToLoadMeshInView { get; set; }
        public int MaxSizeMBToLoadMeshInView { get; set; }

        public bool EnableReduceThumbnailResolution { get; set; }
        public bool EnableReduceThumbnailQuality { get; set; }

        #endregion Advanced settings

        public void LoadSettings()
        {
            var userSettings = DefaultFactory.GetDefaultUserSettings();

            LinkedProgramsData = new ObservableCollection<LinkedProgramData>(userSettings.GetSettingSerialized<List<LinkedProgramData>>(UserSettingEnum.ConfigLinkedProgramsList) ?? new List<LinkedProgramData>());
            EnableDebugLogs = userSettings.GetSettingBool(UserSettingEnum.EnableDebugLogs);
            CachePath = (CacheEnums.CachePathType)userSettings.GetSettingInt(UserSettingEnum.PreferredCachePath);
            SelectedThumbnailRenderAspec = (RenderAspectEnum)userSettings.GetSettingInt(UserSettingEnum.Thumbnails3DAspect);
            EnableThumnailColorsByShaders = userSettings.GetSettingBool(UserSettingEnum.EnableThumnailColorsByShaders);
            EnableChangingViewColorChangesThumnailColor = userSettings.GetSettingBool(UserSettingEnum.EnableChangingViewColorChangesThumnailColor);

            EnableMeshDecimation = userSettings.GetSettingBool(UserSettingEnum.EnableMeshDecimation);
            MinTrianglesForMeshDecimation = userSettings.GetSettingInt(UserSettingEnum.MinTrianglesForMeshDecimation);
            EnableMaxSizeMBToLoadMeshInView = userSettings.GetSettingBool(UserSettingEnum.EnableMaxSizeMBToLoadMeshInView);
            MaxSizeMBToLoadMeshInView = userSettings.GetSettingInt(UserSettingEnum.MaxSizeMBToLoadMeshInView);

            EnableReduceThumbnailResolution = userSettings.GetSettingBool(UserSettingEnum.EnableReduceThumbnailResolution);
            EnableReduceThumbnailQuality = userSettings.GetSettingBool(UserSettingEnum.EnableReduceThumbnailQuality);


            NotifyPropertyChanged(nameof(LinkedProgramsData));
            NotifyPropertyChanged(nameof(EnableDebugLogs));
            NotifyPropertyChanged(nameof(CachePath));
            NotifyPropertyChanged(nameof(SelectedThumbnailRenderAspec));
            NotifyPropertyChanged(nameof(EnableThumnailColorsByShaders));
            NotifyPropertyChanged(nameof(EnableChangingViewColorChangesThumnailColor));
            NotifyPropertyChanged(nameof(EnableMeshDecimation));
            NotifyPropertyChanged(nameof(MinTrianglesForMeshDecimation));
            NotifyPropertyChanged(nameof(EnableMaxSizeMBToLoadMeshInView));
            NotifyPropertyChanged(nameof(MaxSizeMBToLoadMeshInView));
            NotifyPropertyChanged(nameof(EnableReduceThumbnailResolution));
            NotifyPropertyChanged(nameof(EnableReduceThumbnailQuality));
        }

        public void SaveSettings()
        {
            var userSettings = DefaultFactory.GetDefaultUserSettings();

            userSettings.SetSettingSerialized<List<LinkedProgramData>>(UserSettingEnum.ConfigLinkedProgramsList, LinkedProgramsData.ToList());
            userSettings.SetSettingBool(UserSettingEnum.EnableDebugLogs, EnableDebugLogs);
            userSettings.SetSettingInt(UserSettingEnum.PreferredCachePath, (int)CachePath);
            userSettings.SetSettingInt(UserSettingEnum.Thumbnails3DAspect, (int)SelectedThumbnailRenderAspec);
            userSettings.SetSettingBool(UserSettingEnum.EnableThumnailColorsByShaders, EnableThumnailColorsByShaders); 
            userSettings.SetSettingBool(UserSettingEnum.EnableChangingViewColorChangesThumnailColor, EnableChangingViewColorChangesThumnailColor); 

            userSettings.SetSettingBool(UserSettingEnum.EnableMeshDecimation, EnableMeshDecimation);
            userSettings.SetSettingInt(UserSettingEnum.MinTrianglesForMeshDecimation, MinTrianglesForMeshDecimation);
            userSettings.SetSettingBool(UserSettingEnum.EnableMaxSizeMBToLoadMeshInView, EnableMaxSizeMBToLoadMeshInView);
            userSettings.SetSettingInt(UserSettingEnum.MaxSizeMBToLoadMeshInView, MaxSizeMBToLoadMeshInView);

            userSettings.SetSettingBool(UserSettingEnum.EnableReduceThumbnailResolution, EnableReduceThumbnailResolution);
            userSettings.SetSettingBool(UserSettingEnum.EnableReduceThumbnailQuality, EnableReduceThumbnailQuality);

        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
