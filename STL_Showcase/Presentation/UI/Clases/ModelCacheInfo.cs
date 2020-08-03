using STL_Showcase.Data.Cache;
using STL_Showcase.Logic.Localization;
using STL_Showcase.Shared.Main;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace STL_Showcase.Presentation.UI.Clases
{
    class ModelCacheInfo : INotifyPropertyChanged
    {
        public ModelCacheInfo()
        {
            cacheObject = DefaultFactory.GetDefaultThumbnailCache();
        }

        private IThumbnailCache cacheObject;
        public string CachePath { get { return cacheObject.GetCurrentCachePath(); } }

        private string _CacheSize = "";
        public string CacheSize {
            get
            {
                return _CacheSize;
            }
            private set
            {
                this._CacheSize = value;
                NotifyPropertyChanged(nameof(CacheSize));
            }
        }

        bool processingCacheSize = false;
        public string CalculateCacheSize(Dispatcher d)
        {
            if (!processingCacheSize)
            {
                processingCacheSize = true;
                Task.Factory.StartNew(new Action(() =>
              {
                  var size = cacheObject.CacheSize();
                  d.Invoke(new Action(() =>
                  {
                      if (size < 0)
                          CacheSize = Loc.GetText("SizeNotAvailable");
                      else
                          CacheSize = string.Format(Loc.GetText("NumberKB"), Math.Round((size / 1024f)));
                  }));
                  processingCacheSize = false;
              }));
            }
            return Loc.GetText("Calculating...");
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
