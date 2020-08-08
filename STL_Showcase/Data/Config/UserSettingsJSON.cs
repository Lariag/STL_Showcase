using Newtonsoft.Json;
using STL_Showcase.Shared.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace STL_Showcase.Data.Config
{
    class UserSettingsJSON : IUserSettings
    {
        #region Properties

        static NLog.Logger logger = NLog.LogManager.GetLogger("Settings");

        public static UserSettingsJSON Instance { get; private set; }

        private static string _SettingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "STLShowcase", "STLShowcase_Settings.json");

        private bool _ConfigChanged = false;

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

        };
        private object[] _CurrentSettingsArray;

        #endregion Properties

        #region Initialization

        static UserSettingsJSON()
        {
            Instance = new UserSettingsJSON();
        }

        private UserSettingsJSON()
        {
            logger.Info("Initializing application settings with {0}.", nameof(UserSettingsJSON));

            LoadSettingsFromFile();

            Task.Run(() => SaveSettingsTask(this));
        }

        ~UserSettingsJSON()
        {
            SaveSettingsTask(this);
        }

        #endregion Initialization

        #region Private Methods

        private void LoadSettingsFromFile()
        {
            if (File.Exists(_SettingsFilePath))
            {
                var loadedObject = JsonConvert.DeserializeObject<object[]>(File.ReadAllText(_SettingsFilePath));
                _CurrentSettingsArray = loadedObject;
                SetDefaultSettings(false);
            }
            else
            {
                _CurrentSettingsArray = new object[_DefaultSettingsArray.Length];
                SetDefaultSettings(true);
                _ConfigChanged = true;
            }
        }

        private async void SaveSettingsTask(UserSettingsJSON ins)
        {
            while (true)
            {
                if (ins._ConfigChanged)
                {
                    try
                    {
                        logger.Info("Saving settings to disk with {0} at {1}", nameof(UserSettingsJSON), _SettingsFilePath);

                        lock (_CurrentSettingsArray)
                        {
                            if (!Directory.Exists(Path.GetDirectoryName(_SettingsFilePath)))
                                Directory.CreateDirectory(Path.GetDirectoryName(_SettingsFilePath));

                            File.WriteAllText(_SettingsFilePath, JsonConvert.SerializeObject(_CurrentSettingsArray));
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Debug(ex, "Error when saving settings to disk with {0} at {1}", nameof(UserSettingsJSON), _SettingsFilePath);
                    }
                    ins._ConfigChanged = false;
                }
                await Task.Delay(10000);
            }
        }

        private void SetSetting(UserSettingEnum setting, object val)
        {
            _ConfigChanged = false;

            _CurrentSettingsArray[(int)setting] = val;

            _ConfigChanged = true;
        }
        private T GetSetting<T>(UserSettingEnum setting)
        {
            object val = _CurrentSettingsArray[(int)setting];
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
            //return GetSetting<T>(setting);
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
            //SetSetting(setting, val);
            SetSetting(setting, JsonConvert.SerializeObject(val));
        }

        public void SetDefaultSettings(bool overrideExisting)
        {
            for (int i = 0; i < _DefaultSettingsArray.Length; i++)
                if (_CurrentSettingsArray[i] == null || overrideExisting)
                    _CurrentSettingsArray[i] = _DefaultSettingsArray[i];
        }


        #endregion Setters

        #endregion IUserSettings

    }
}
