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
                case UserSettingEnum.Language:
                    return Properties.Settings.Default.Language;
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
                case UserSettingEnum.MainColumnsPoweredIndex:
                    return Properties.Settings.Default.MainColumnsPoweredIndex;
                default:
                    return 0;
            }
        }

        public bool GetSettingBool(UserSettingEnum setting)
        {
            switch (setting)
            {
                case UserSettingEnum.MainColumnsVisibilityDirectoryTree:
                    return Properties.Settings.Default.MainColumnsVisibilityDirectoryTree;
                case UserSettingEnum.MainColumnsVisibilityModelList:
                    return Properties.Settings.Default.MainColumnsVisibilityModelList;
                case UserSettingEnum.MainColumnsVisibility3DView:
                    return Properties.Settings.Default.MainColumnsVisibility3DView;
                default:
                    return false;
            }
        }

        public float GetSettingFloat(UserSettingEnum setting)
        {
            throw new NotImplementedException("Float setting not implemented yet.");
        }

        public void SetSettingString(UserSettingEnum setting, string val)
        {
            switch (setting)
            {
                case UserSettingEnum.LastDirectory:
                    Properties.Settings.Default.LastDirectory = val; break;
                case UserSettingEnum.Language:
                    Properties.Settings.Default.Language = val; break;
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
                case UserSettingEnum.MainColumnsPoweredIndex:
                    Properties.Settings.Default.MainColumnsPoweredIndex = val; break;
            }
            Properties.Settings.Default.Save();
        }

        public void SetSettingBool(UserSettingEnum setting, bool val)
        {
            switch (setting)
            {
                case UserSettingEnum.MainColumnsVisibilityDirectoryTree:
                    Properties.Settings.Default.MainColumnsVisibilityDirectoryTree = val; break;
                case UserSettingEnum.MainColumnsVisibilityModelList:
                    Properties.Settings.Default.MainColumnsVisibilityModelList = val; break;
                case UserSettingEnum.MainColumnsVisibility3DView:
                    Properties.Settings.Default.MainColumnsVisibility3DView = val; break;
            }
            Properties.Settings.Default.Save();
        }

        public void SetSettingFloat(UserSettingEnum setting, float val)
        {
            throw new NotImplementedException("Float setting not implemented yet.");
            Properties.Settings.Default.Save();
        }
    }
}
