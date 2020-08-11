using STL_Showcase.Logic.Files;
using STL_Showcase.Shared.Enums;
using STL_Showcase.Shared.Main;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
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

        private BitmapScalingMode _scalingMode;
        public BitmapScalingMode ScalingMode { get { return _scalingMode; } set { _scalingMode = value; NotifyPropertyChanged(nameof(ScalingMode)); } }
        public string ErrorMessage { get; set; }
        private static int MinImageSize { get; set; } = 32;

        bool _IsSelected;
        public bool IsSelected {
            get { return _IsSelected; }
            set
            {
                _IsSelected = value;

                if (!IsSelected)
                    _BackgroundColor = Colors.White;
                else
                    switch ((RenderAspectEnum)DefaultFactory.GetDefaultUserSettings().GetSettingInt(Shared.Enums.UserSettingEnum.Thumbnails3DAspect))
                    {
                        default:
                        case RenderAspectEnum.PerNormal:
                            _BackgroundColor = Color.FromRgb(186, 0, 12); break;
                        case RenderAspectEnum.CyanBlue:
                            _BackgroundColor = Color.FromRgb(170, 170, 0); break;
                        case RenderAspectEnum.GreenLimeYellow:
                            _BackgroundColor = Color.FromRgb(76, 48, 171); break;
                        case RenderAspectEnum.PinkFucsiaViolet:
                            _BackgroundColor = Color.FromRgb(39, 179, 115); break;
                        case RenderAspectEnum.RedOrangeYellow:
                            _BackgroundColor = Color.FromRgb(27, 176, 92); break;
                        case RenderAspectEnum.RedRedish:
                            _BackgroundColor = Color.FromRgb(75, 186, 28); break;
                        case RenderAspectEnum.VioletBlue:
                            _BackgroundColor = Color.FromRgb(38, 183, 41); break;
                        case RenderAspectEnum.White:
                            _BackgroundColor = Colors.Black; break;
                        case RenderAspectEnum.Yellow:
                            _BackgroundColor = Color.FromRgb(153, 65, 126); break;
                    }

                NotifyPropertyChanged(nameof(IsSelected)); NotifyPropertyChanged(nameof(BackgroundBrush));
            }
        }

        private Color _BackgroundColor;
        public Brush BackgroundBrush { get { return new SolidColorBrush(_BackgroundColor); } }

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
