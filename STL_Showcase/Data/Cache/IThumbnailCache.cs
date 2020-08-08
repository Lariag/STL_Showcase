using STL_Showcase.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static STL_Showcase.Shared.Enums.CacheEnums;

namespace STL_Showcase.Data.Cache
{
    public interface IThumbnailCache
    {

        IEnumerable<Tuple<int, BitmapSource>> GetThumbnailsImages(string filePath, string fileName, RenderAspectEnum renderType);
        BitmapSource GetThumbnailImage(string filePath, string fileName, RenderAspectEnum renderType, int thumbnailSize);

        IEnumerable<Tuple<int, string>> GetThumbnailsPaths(string filePath, string fileName, RenderAspectEnum renderType);
        bool CheckThumbnailExists(string filePath, string fileName, RenderAspectEnum renderType, int thumbnailSize);


        bool UpdateThumnail(string filePath, string fileName, RenderAspectEnum renderType, int size, BitmapSource data);
        /// <summary>
        /// Deletes all cache files for current cache mode/folder.
        /// </summary>
        /// <returns>True if all files were cleared or no files were found. False if was unable to delete one or more files.</returns>
        bool ClearCache();

        /// <summary>
        /// Deletes the cache files for the received filenames.
        /// </summary>
        /// <param name="files">Filenames (without path, but with extension)</param>
        /// <returns>True if all files were cleared or no files were found.  False if was unable to delete one or more files.</returns>
        bool ClearCacheForFiles(IEnumerable<string> filenames);

        /// <summary>
        /// Calculates the current cache size in bytes. May not be supported by all cache types.
        /// </summary>
        /// <returns>Cache size in bytes. 0 means no chache currently generated. -1 means operation not supported.</returns>
        long CacheSize();

        /// <summary>
        /// Moves all exsisting cache files and folders from current location to the new one.
        /// </summary>
        /// <param name="oldPath">Origin location.</param>
        /// <param name="newPath">Destination location.</param>
        /// <returns>Bool if success, false if any problem occured.</returns>
        bool MoveCacheToNewLocation(CachePathType oldPath, CachePathType newPath);

        /// <summary>
        /// Gets the full path for the current cache directory.
        /// </summary>
        string GetCurrentCachePath();
    }
}
