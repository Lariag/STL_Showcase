using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STL_Showcase.Data.Cache
{
    public class ThumbnailCacheFactory
    {

        static NLog.Logger logger = NLog.LogManager.GetLogger("Cache");

        public enum CacheType
        {
            FolderImages
            , ShellCache // Planned a shell extension to generate thumbnails and get them from windows.
        }

        public static IThumbnailCache GetCache(CacheType t)
        {
            logger.Info("Creating instance of {cacheType}", t.ToString());
            switch (t)
            {
                case CacheType.FolderImages:
                    return ThumbnailCacheInFolder.Instance;
                default:
                    throw new NotImplementedException($"Thumbnail Cache for {t.ToString()} is not implemented.");
            }
        }
    }
}
