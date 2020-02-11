using STL_Showcase.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STL_Showcase.Data.Config
{
    class UserSettingsNET : IUserSettings
    {
        public static UserSettingsNET Instance { get; private set; }

        static UserSettingsNET()
        {
            Instance = new UserSettingsNET();
        }

        public string GetSettingString(UserSettingEnum setting)
        {
            switch (setting)
            {
                case UserSettingEnum.LastDirectory:
                    return Properties.Settings.Default.LastDirectory;
                default:
                    return "";
            }
        }
        public int GetSettingInt(UserSettingEnum setting)
        {
            switch (setting)
            {
                case UserSettingEnum.PreferredCachePath:
                    return Properties.Settings.Default.PreferredCachePath;
                case UserSettingEnum.RenderAspect:
                    return Properties.Settings.Default.RenderAspect;
                case UserSettingEnum.CurrentView3DAspect:
                    return Properties.Settings.Default.CurrentView3DAspect;
                default:
                    return 0;
            }
        }
        public float GetSettingFloat(UserSettingEnum setting)
        {
            return 0f;
        }
        public void SetSettingString(UserSettingEnum setting, string val)
        {
            switch (setting)
            {
                case UserSettingEnum.LastDirectory:
                    Properties.Settings.Default.LastDirectory = val; break;
            }
            Properties.Settings.Default.Save();
        }
        public void SetSettingInt(UserSettingEnum setting, int val)
        {
            switch (setting)
            {
                case UserSettingEnum.PreferredCachePath:
                    Properties.Settings.Default.PreferredCachePath = val; break;
                case UserSettingEnum.RenderAspect:
                    Properties.Settings.Default.RenderAspect = val; break;
                case UserSettingEnum.CurrentView3DAspect:
                    Properties.Settings.Default.CurrentView3DAspect = val; break;
            }
            Properties.Settings.Default.Save();
        }
        public void SetSettingFloat(UserSettingEnum setting, float val)
        {
            Properties.Settings.Default.Save();
        }
    }
}
