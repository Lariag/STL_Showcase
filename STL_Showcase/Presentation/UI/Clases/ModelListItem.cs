using STL_Showcase.Logic.Files;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace STL_Showcase.Presentation.UI.Clases
{
    public class ModelListItem : INotifyPropertyChanged
    {

        public int ZoomLevel { get; set; } = 0;
        private BitmapSource[] _imagesAllLevels { get; set; }
        public ModelFileData FileData { get; set; }
        public string ImagePath { get; set; }
        public string Text { get { return FileData.FileName; } }
        public BitmapSource Image => _imagesAllLevels?.LastOrDefault(); // _imagesAllLevels?.FirstOrDefault(img => img.PixelWidth >= MinImageSize) ?? _imagesAllLevels?.LastOrDefault();
        public BitmapSource ImageSmallest { get { return _imagesAllLevels.FirstOrDefault(); } }

        private static int MinImageSize { get; set; } = 32;

        bool _IsSelected;
        public bool IsSelected { get { return _IsSelected; } set { _IsSelected = value; NotifyPropertyChanged(nameof(IsSelected)); } }


        public void SetImages(BitmapSource[] images)
        {
            _imagesAllLevels = images;
            NotifyPropertyChanged(nameof(Image));
            NotifyPropertyChanged(nameof(ImageSmallest));
        }

        public static void SetImageSizeFor(int size)
        {
            MinImageSize = size;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
