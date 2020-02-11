using STL_Showcase.Data.Config;
using STL_Showcase.Shared.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static STL_Showcase.Shared.Enums.CacheEnums;

namespace STL_Showcase.Data.Cache {
    class ThumbnailCacheInFolder : IThumbnailCache {

        #region Fields

        public static ThumbnailCacheInFolder Instance { get; private set; }
        static NLog.Logger logger = NLog.LogManager.GetLogger( "Cache" );
        const string CacheFolderName = "STLShowcaseImageCache";
        IUserSettings settings;

        #endregion

        #region Constructors

        static ThumbnailCacheInFolder() {
            Instance = new ThumbnailCacheInFolder();
        }

        private ThumbnailCacheInFolder() {
            settings = UserSettingsFactory.GetSettings( UserSettingsFactory.SettingsType.RegularSettings );
        }

        #endregion

        #region IThumbnailCache members

        public long CacheSize() {
            long size = 0;
            string cachePath = GetCachePath();

            try {
                logger.Info( "Calculating cache size..." );
                if(Directory.Exists( cachePath )) {
                    foreach(var filePath in Directory.EnumerateFiles( cachePath, "*.png", SearchOption.AllDirectories )) {
                        using(var file = File.OpenRead( filePath )) {
                            size += file.Length;
                        }
                    }
                }
            }
            catch(Exception ex) {
                logger.Trace( ex, "Exception calculating cache size" );
                throw ex;
            }
            logger.Info( "Calculated cache size: {size}", size );

            return size;
        }

        public bool ClearCache() {
            string cachePath = GetCachePath();
            bool allFilesCleared = true;

            try {
                if(Directory.Exists( cachePath )) {
                    var files = Directory.EnumerateFiles( cachePath, "*.png", SearchOption.AllDirectories );
                    logger.Info( "Clearing cache. Detected {fileCount} files.", files.Count() );

                    foreach(var filePath in files) {
                        try {
                            File.Delete( filePath );
                        }
                        catch(Exception ex) {
                            allFilesCleared = false;
                            logger.Trace( ex, "Error deleting cache file: {filePath}", filePath );
                        }
                    }
                }
                else {
                    logger.Info( "Clearing cache... Cache path not found! {cachePath}", cachePath );
                }
            }
            catch(Exception ex) {
                logger.Trace( ex, "Exception calculating cache size" );
                throw ex;
            }
            return allFilesCleared;
        }

        public IEnumerable<Tuple<int, byte[]>> GetThumbnails(string filePath, string fileName, RenderAspectEnum renderType) {
            Tuple<int, byte[]>[] loadedFiles = null;
            string cachePath = GetCachePath();
            string filesNameForFilter = GetComposedFileNameForFilter( fileName, renderType );

            try {
                if(Directory.Exists( cachePath )) {
                    var files = Directory.EnumerateFiles( cachePath, filesNameForFilter, SearchOption.AllDirectories );

                    loadedFiles = new Tuple<int, byte[]>[files.Count()];
                    int i = 0;
                    foreach(var foundFile in files) {
                        using(var file = File.OpenRead( foundFile )) {
                            loadedFiles[i] = new Tuple<int, byte[]>( GetFileSizeFromFileName( foundFile ), new byte[file.Length] );
                            file.Read( loadedFiles[i].Item2, 0, (int)file.Length );
                        }
                        i++;
                    }
                }

            }
            catch(Exception ex) {
                logger.Trace( ex, "Exception loading a cache files for {fullpath} as {renderType}", Path.Combine( filePath, fileName ), renderType.ToString() );
                throw ex;
            }

            if(loadedFiles == null) logger.Info( "No cache files found for {filePath} as {filesNameForFilter}", Path.Combine( filePath, fileName ), filesNameForFilter );
            else logger.Info( "Loaded {fileCount} cache files found for {filePath} as {filesNameForFilter}", loadedFiles.Length, Path.Combine( filePath, fileName ), filesNameForFilter );

            return loadedFiles;
        }

        public bool UpdateThumnail(string filePath, string fileName, RenderAspectEnum renderType, int size, byte[] data) {
            bool updated = true;
            string cachePath = GetCachePath( size );
            string cachedFileName = ComposeFileName( fileName, renderType, size );
            string fullCachedFilePath = Path.Combine( cachePath, cachedFileName );

            try {
                if(!Directory.Exists( cachePath )) {
                    Directory.CreateDirectory( cachePath );
                    logger.Info( "Created cache directory at {cachePath}", cachePath );
                }
            }
            catch(Exception ex) {
                logger.Trace( ex, "Exception creating the cache directory at  {cachePath}", cachePath );
                throw ex;
            }

            try {
                using(var file = File.OpenWrite( fullCachedFilePath )) {
                    file.Write( data, 0, data.Length );
                }
                logger.Info( "Updated file thumbnail for {fileName} for size {size}", fileName, size );
            }
            catch(Exception ex) {
                logger.Trace( ex, "Exception updating a file thumbnail at {fullCachedFilePath} for size {size} ", fullCachedFilePath, size );
                throw ex;
            }

            return updated;
        }

        #endregion

        #region Private methods

        private string GetCachePath(int size = 0) {
            string cachePath = "";

            CachePathType cachePathLocation = (CachePathType)settings.GetSettingInt( Shared.Enums.UserSettingEnum.PreferredCachePath );
            switch(cachePathLocation) {
                case CachePathType.ApplicationFolder:
                    cachePath = Path.Combine( AppContext.BaseDirectory, CacheFolderName );
                    break;
                case CachePathType.UserDataFolder:
                    cachePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), CacheFolderName );
                    break;
                case CachePathType.UserImagesFolder:
                    cachePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyPictures ), CacheFolderName );
                    break;
            }

            if(size > 0)
                cachePath = Path.Combine( cachePath, size.ToString() );
            logger.Info( "Using cache folder {cachePath}", cachePath );

            return cachePath;
        }

        private string ComposeFileName(string fileName, RenderAspectEnum renderType, int size) {
            return string.Format( $"{fileName}.cached.{(int)renderType}.{size}.png" );
        }
        private string GetComposedFileNameForFilter(string fileName, RenderAspectEnum renderType) {
            return string.Format( $"{fileName}.cached.{(int)renderType}.*.png" );
        }
        private int GetFileSizeFromFileName(string fileName) {
            if(int.TryParse( Regex.Match( fileName, @"\.\d+\.(\d+)\.png*" ).Groups[1].Value, out int matched ))
                return matched;
            return 0;
        }
        #endregion
    }
}
