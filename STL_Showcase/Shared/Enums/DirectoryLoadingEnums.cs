using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STL_Showcase.Shared.Enums
{
    public enum DirectoryLoadingStepsEnum
    {
        Path = 0,
        CacheCheck,
        FileBasicDataLoading,
        FileBytesLoading,
        FileParsing,
        FileRendering,
        CacheSaving,
        Ready
    }
    public enum LoadResultEnum
    {
        Okay = 0,
        ErrorReadingFileData,
        ErrorLoadingThumbnails,
        ErrorAddingThumbnailsToList,
        ErrorReadingFileBytes,
        ErrorParsing,
        ErrorRendering,
        NotEnoughFreeMemory,
        ErrorRetrievingLoadedCache,
        ErrorRetrievingRenderedCache,
        ErrorSavingCacheFile
    }
}
