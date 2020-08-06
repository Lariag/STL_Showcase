using Newtonsoft.Json;
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
                case UserSettingEnum.LastLoadedDirectories:
                    return Properties.Settings.Default.LastLoadedDirectories;
                default:
                    return string.Empty;
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
                case UserSettingEnum.Thumbnails3DAspect:
                    return Properties.Settings.Default.Thumbnails3DAspect;
                case UserSettingEnum.MinTrianglesForMeshDecimation:
                    return Properties.Settings.Default.MinTrianglesForMeshDecimation;
                case UserSettingEnum.MaxSizeMBToLoadMeshInView:
                    return Properties.Settings.Default.MaxSizeMBToLoadMeshInView;
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
                case UserSettingEnum.EnableDebugLogs:
                    return Properties.Settings.Default.EnableDebugLogs;
                case UserSettingEnum.EnableTreeCollections:
                    return Properties.Settings.Default.EnableTreeCollections;
                case UserSettingEnum.EnableTreeOnlyFolders:
                    return Properties.Settings.Default.EnableTreeOnlyFolders;
                case UserSettingEnum.EnableMeshDecimation:
                    return Properties.Settings.Default.EnableMeshDecimation;
                case UserSettingEnum.EnableMaxSizeMBToLoadMeshInView:
                    return Properties.Settings.Default.EnableMaxSizeMBToLoadMeshInView;
                case UserSettingEnum.EnableReduceThumbnailQuality:
                    return Properties.Settings.Default.EnableReduceThumbnailQuality;
                case UserSettingEnum.EnableReduceThumbnailResolution:
                    return Properties.Settings.Default.EnableReduceThumbnailResolution;
                default:
                    return false;
            }
        }

        public float GetSettingFloat(UserSettingEnum setting)
        {
            throw new NotImplementedException("Float setting not implemented yet.");
        }

        public T GetSettingSerialized<T>(UserSettingEnum setting)
        {
            string objectSerialized = string.Empty;
            switch (setting)
            {
                case UserSettingEnum.ConfigLinkedProgramsList:
                    objectSerialized = Properties.Settings.Default.ConfigLinkedProgramsList; break;
                default:
                    objectSerialized = string.Empty; break;
            }

            return string.IsNullOrEmpty(objectSerialized) ? default(T) : JsonConvert.DeserializeObject<T>(objectSerialized);
        }

        public void SetSettingString(UserSettingEnum setting, string val)
        {
            switch (setting)
            {
                case UserSettingEnum.LastDirectory:
                    Properties.Settings.Default.LastDirectory = val; break;
                case UserSettingEnum.Language:
                    Properties.Settings.Default.Language = val; break;
                case UserSettingEnum.LastLoadedDirectories:
                    Properties.Settings.Default.LastLoadedDirectories = val; break;
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
                case UserSettingEnum.Thumbnails3DAspect:
                    Properties.Settings.Default.Thumbnails3DAspect = val; break;
                case UserSettingEnum.MinTrianglesForMeshDecimation:
                    Properties.Settings.Default.MinTrianglesForMeshDecimation = val; break;
                case UserSettingEnum.MaxSizeMBToLoadMeshInView:
                    Properties.Settings.Default.MaxSizeMBToLoadMeshInView = val; break;
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
                case UserSettingEnum.EnableDebugLogs:
                    Properties.Settings.Default.EnableDebugLogs = val; break;
                case UserSettingEnum.EnableTreeCollections:
                    Properties.Settings.Default.EnableTreeCollections = val; break;
                case UserSettingEnum.EnableTreeOnlyFolders:
                    Properties.Settings.Default.EnableTreeOnlyFolders = val; break;
                case UserSettingEnum.EnableMeshDecimation:
                    Properties.Settings.Default.EnableMeshDecimation = val; break;
                case UserSettingEnum.EnableMaxSizeMBToLoadMeshInView:
                    Properties.Settings.Default.EnableMaxSizeMBToLoadMeshInView = val; break;
                case UserSettingEnum.EnableReduceThumbnailResolution:
                    Properties.Settings.Default.EnableReduceThumbnailResolution = val; break;
                case UserSettingEnum.EnableReduceThumbnailQuality:
                    Properties.Settings.Default.EnableReduceThumbnailQuality = val; break;
            }
            Properties.Settings.Default.Save();
        }

        public void SetSettingFloat(UserSettingEnum setting, float val)
        {
            throw new NotImplementedException("Float setting not implemented yet.");
            Properties.Settings.Default.Save();
        }

        public void SetSettingSerialized<T>(UserSettingEnum setting, T val)
        {
            switch (setting)
            {
                case UserSettingEnum.ConfigLinkedProgramsList:
                    Properties.Settings.Default.ConfigLinkedProgramsList = JsonConvert.SerializeObject(val); break;
            }
            Properties.Settings.Default.Save();
        }
    }
}
