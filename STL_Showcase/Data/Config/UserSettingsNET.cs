using Newtonsoft.Json;
using STL_Showcase.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace STL_Showcase.Data.Config
{
    class UserSettingsNET : IUserSettings
    {
        #region Properties

        static NLog.Logger logger = NLog.LogManager.GetLogger("Settings");

        public static UserSettingsNET Instance { get; private set; }

        private bool _ConfigChanged = false;

        private readonly Dictionary<UserSettingEnum, PropertyInfo> _PropertyInfoDictinary = new Dictionary<UserSettingEnum, PropertyInfo>();

        private object[] _DefaultSettingsArray = {
              0                 // PreferredCachePath
            , "en"              // Language
            , null              // LastDirectory
            , 0                 // RenderAspect
            , 0                 // CurrentView3DAspect
            , true              // MainColumnsVisibilityDirectoryTree
            , true              // MainColumnsVisibilityModelList
            , true              // MainColumnsVisibility3DView
            , -1                // MainColumnsPoweredIndex
            , null              // LastLoadedDirectories
            , null              // ConfigLinkedProgramsList
            , true              // EnableDebugLogs
            , 0                 // Thumbnails3DAspect
            , false             // EnableTreeCollections
            , false             // EnableTreeOnlyFolders
            , false             // EnableMeshDecimation
            , 60000             // MinTrianglesForMeshDecimation
            , 50                // MaxSizeMBToLoadMeshInView
            , false             // EnableMaxSizeMBToLoadMeshInView
            , false             // EnableReduceThumbnailResolution
            , false             // EnableReduceThumbnailQuality
            , true              // EnableViewModelAutoRotation
            , true              // EnableThumnailColorsByShaders
            , true              // EnableChangingViewColorChangesThumnailColor
        };

        #endregion Properties

        #region Initialization

        static UserSettingsNET()
        {
            Instance = new UserSettingsNET();
        }

        private UserSettingsNET()
        {
            logger.Info("Initializing application settings with {0}.", nameof(UserSettingsNET));

            string[] enumNames = Enum.GetNames(typeof(UserSettingEnum));
            foreach (var prop in Properties.Settings.Default.GetType().GetProperties().Where(p => enumNames.Contains(p.Name)))
            {
                UserSettingEnum propEnum = (UserSettingEnum)Enum.Parse(typeof(UserSettingEnum), prop.Name);
                _PropertyInfoDictinary.Add(propEnum, prop);
            }
            Task.Run(() => SaveSettingsTask(this));
        }

        ~UserSettingsNET()
        {
            Properties.Settings.Default.Save();
        }

        #endregion Initialization

        #region Private Methods

        private async void SaveSettingsTask(UserSettingsNET ins)
        {
            while (true)
            {
                if (ins._ConfigChanged)
                {
                    Properties.Settings.Default.Save();
                    ins._ConfigChanged = false;
                    logger.Info("Saving settings to disk with {0}.", nameof(UserSettingsNET));
                }
                await Task.Delay(10000);
            }
        }

        private void SetSetting(UserSettingEnum setting, object val)
        {
            _ConfigChanged = false;

            if (_PropertyInfoDictinary.TryGetValue(setting, out PropertyInfo prop))
                prop.SetValue(Properties.Settings.Default, val);

            _ConfigChanged = true;
        }
        private T GetSetting<T>(UserSettingEnum setting)
        {

            _PropertyInfoDictinary.TryGetValue(setting, out PropertyInfo prop);
            object val = prop.GetValue(Properties.Settings.Default);

            T valConverted = val == null ? default(T) : (T)Convert.ChangeType(val, typeof(T));
            return valConverted;

        }

        #endregion Private Methods

        #region IUserSettings

        #region Getters

        public string GetSettingString(UserSettingEnum setting)
        {
            return GetSetting<string>(setting) ?? string.Empty;
        }

        public int GetSettingInt(UserSettingEnum setting)
        {
            return GetSetting<int>(setting);
        }

        public bool GetSettingBool(UserSettingEnum setting)
        {
            return GetSetting<bool>(setting);
        }

        public float GetSettingFloat(UserSettingEnum setting)
        {
            return GetSetting<float>(setting);
        }

        public T GetSettingSerialized<T>(UserSettingEnum setting)
        {
            string objectSerialized = GetSetting<string>(setting) ?? string.Empty;
            return string.IsNullOrEmpty(objectSerialized) ? default : JsonConvert.DeserializeObject<T>(objectSerialized);
        }

        #endregion Getters

        #region Setters

        public void SetSettingString(UserSettingEnum setting, string val)
        {
            SetSetting(setting, val);
        }

        public void SetSettingInt(UserSettingEnum setting, int val)
        {
            SetSetting(setting, val);
        }

        public void SetSettingBool(UserSettingEnum setting, bool val)
        {
            SetSetting(setting, val);
        }

        public void SetSettingFloat(UserSettingEnum setting, float val)
        {
            SetSetting(setting, val);
        }

        public void SetSettingSerialized<T>(UserSettingEnum setting, T val)
        {
            SetSetting(setting, JsonConvert.SerializeObject(val));
        }

        public void SetDefaultSettings(bool overrideExisting)
        {
            if (!overrideExisting)
                throw new NotImplementedException("Not overriding option is not implemented. Can only call UserSettingsNET.SetDefaultSettings() with overrideExisting = true.");

            var enumSettings = (UserSettingEnum[])Enum.GetValues(typeof(UserSettingEnum));
            for (int i = 0; i < enumSettings.Length; i++)
            {
                SetSetting(enumSettings[i], _DefaultSettingsArray[(int)enumSettings[i]]);
            }
        }
        #endregion Setters

        #endregion IUserSettings

    }
}
