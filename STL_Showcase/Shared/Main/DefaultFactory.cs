using STL_Showcase.Data.Cache;
using STL_Showcase.Data.Config;
using STL_Showcase.Logic.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STL_Showcase.Shared.Main
{
    public class DefaultFactory
    {

        private const ThumbnailCacheFactory.CacheType ThumbnailCacheType = ThumbnailCacheFactory.CacheType.FolderImages;
        private const RenderEnviorementFactory.EvnType RenderEnviorementType = RenderEnviorementFactory.EvnType.OpenTx;
        private const UserSettingsFactory.SettingsType UserSettingsType = UserSettingsFactory.SettingsType.RegularSettings;

        public static IThumbnailCache GetDefaultThumbnailCache()
        {
            return ThumbnailCacheFactory.GetCache(ThumbnailCacheType);
        }
        public static IRenderEnviorement GetDefaultRenderEnviorement(int renderResolution)
        {
            return RenderEnviorementFactory.CreateEnviorement(RenderEnviorementType, renderResolution);
        }
        public static IUserSettings GetDefaultUserSettings()
        {
            return UserSettingsFactory.GetSettings(UserSettingsType);
        }
    }
}
