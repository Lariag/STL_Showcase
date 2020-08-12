using STL_Showcase.Data.Config;
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
using System.Windows.Media.Effects;
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

        private static ShaderEffectNormalColoring _ColoredImageEffect = new ShaderEffectNormalColoring();
        private static ShaderEffectNormalColoring _ColoredImageEffectInverted = new ShaderEffectNormalColoring();
        private static ShaderEffectInvert _ImageEffectInverted = new ShaderEffectInvert();

        public bool IsSelected {
            get { return _IsSelected; }
            set
            {
                _IsSelected = value;
                IUserSettings userSettings = DefaultFactory.GetDefaultUserSettings();
                bool useColorByShader = userSettings.GetSettingBool(UserSettingEnum.EnableThumnailColorsByShaders);

                if (!IsSelected)
                {
                    _BackgroundColor = Colors.White;
                }
                else
                {
                    RenderAspectEnum renderAspect = (RenderAspectEnum)userSettings.GetSettingInt(Shared.Enums.UserSettingEnum.Thumbnails3DAspect);
                    switch (renderAspect)
                    {
                        default:
                            _BackgroundColor = Colors.White; break;
                        case RenderAspectEnum.PerNormal:
                            _BackgroundColor = Colors.AliceBlue; break;
                        case RenderAspectEnum.CyanBlue:
                            _BackgroundColor = Color.FromRgb(18, 193, 198); break;
                        case RenderAspectEnum.GreenLimeYellow:
                            _BackgroundColor = Color.FromRgb(107, 198, 18); break;
                        case RenderAspectEnum.PinkFucsiaViolet:
                            _BackgroundColor = Color.FromRgb(221, 112, 218); break;
                        case RenderAspectEnum.RedOrangeYellow:
                            _BackgroundColor = Color.FromRgb(198, 172, 48); break;
                        case RenderAspectEnum.RedRedish:
                            _BackgroundColor = Color.FromRgb(225, 53, 27); break;
                        case RenderAspectEnum.VioletBlue:
                            _BackgroundColor = Color.FromRgb(125, 67, 198); break;
                        case RenderAspectEnum.White:
                            _BackgroundColor = Colors.Black; break;
                        case RenderAspectEnum.Yellow:
                            _BackgroundColor = Color.FromRgb(174, 198, 23); break;
                    }
                }

                NotifyPropertyChanged(nameof(IsSelected)); NotifyPropertyChanged(nameof(BackgroundBrush)); NotifyPropertyChanged(nameof(ImageEffect));
            }
        }

        private ShaderEffect _ImageEffectUnselected;
        private ShaderEffect _ImageEffectSelected;
        public ShaderEffect ImageEffect {
            get
            {
                if (!string.IsNullOrEmpty(ErrorMessage))
                    return null;
                return IsSelected ? _ImageEffectSelected : _ImageEffectUnselected;
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

        public void EnableShaderImageEffect(bool enabled)
        {
            this._ImageEffectUnselected = enabled ? _ColoredImageEffect : null;
            this._ImageEffectSelected = enabled ? (ShaderEffect)_ColoredImageEffectInverted : (ShaderEffect)_ImageEffectInverted;
            NotifyPropertyChanged(nameof(ImageEffect));
        }

        public static void SetImageSizeFor(int size)
        {
            MinImageSize = size;
        }

        public static void SetListItemColoring(RenderAspectEnum renderAspect)
        {
            switch (renderAspect)
            {
                default:
                case RenderAspectEnum.PerNormal:
                    {
                        byte w = Convert.ToByte(255f * 0.50f);
                        _ColoredImageEffect.ColorY = Color.FromRgb(w, 255, w);
                        _ColoredImageEffect.ColorX = Color.FromRgb(255, w, w);
                        _ColoredImageEffect.ColorZ = Color.FromRgb(w, w, 255);
                    }
                    break;
                case RenderAspectEnum.White:
                    {
                        byte w = Convert.ToByte(255f * 0.6f);
                        _ColoredImageEffect.ColorY = Color.FromRgb(w, w, w);
                        _ColoredImageEffect.ColorX = Color.FromRgb(w, w, w);
                        _ColoredImageEffect.ColorZ = Color.FromRgb(w, w, w);
                    }
                    break;
                case RenderAspectEnum.VioletBlue:
                    _ColoredImageEffect.ColorY = Colors.Blue;
                    _ColoredImageEffect.ColorX = Colors.Violet;
                    _ColoredImageEffect.ColorZ = Color.FromRgb(150, 80, 238);
                    break;
                case RenderAspectEnum.RedOrangeYellow:
                    _ColoredImageEffect.ColorY = Colors.Red;
                    _ColoredImageEffect.ColorX = Colors.Yellow;
                    _ColoredImageEffect.ColorZ = Colors.Orange;
                    break;
                case RenderAspectEnum.GreenLimeYellow:
                    _ColoredImageEffect.ColorY = Colors.Green;
                    _ColoredImageEffect.ColorX = Colors.Yellow;
                    _ColoredImageEffect.ColorZ = Colors.LimeGreen;
                    break;
                case RenderAspectEnum.PinkFucsiaViolet:
                    _ColoredImageEffect.ColorY = Colors.Pink;
                    _ColoredImageEffect.ColorX = Color.FromArgb(1, 150, 40, 80);
                    _ColoredImageEffect.ColorZ = Colors.Violet;
                    break;
                case RenderAspectEnum.CyanBlue:
                    _ColoredImageEffect.ColorY = Color.FromRgb(0, 0, 255);
                    _ColoredImageEffect.ColorX = Color.FromRgb(100, 150, 234);
                    _ColoredImageEffect.ColorZ = Color.FromRgb(0, 255, 255);
                    break;
                case RenderAspectEnum.RedRedish:
                    _ColoredImageEffect.ColorY = Colors.Red;
                    _ColoredImageEffect.ColorX = Colors.DarkRed;
                    _ColoredImageEffect.ColorZ = Colors.PaleVioletRed;
                    break;
                case RenderAspectEnum.Yellow:
                    _ColoredImageEffect.ColorY = Colors.Yellow;
                    _ColoredImageEffect.ColorX = Colors.YellowGreen;
                    _ColoredImageEffect.ColorZ = Color.FromArgb(1, 230, 230, 130);
                    break;
            }

            if (renderAspect != RenderAspectEnum.PerNormal)
            {
                _ColoredImageEffectInverted.ColorX = Color.FromRgb((byte)(255 - _ColoredImageEffect.ColorX.R), (byte)(255 - _ColoredImageEffect.ColorX.G), (byte)(255 - _ColoredImageEffect.ColorX.B));
                _ColoredImageEffectInverted.ColorY = Color.FromRgb((byte)(255 - _ColoredImageEffect.ColorY.R), (byte)(255 - _ColoredImageEffect.ColorY.G), (byte)(255 - _ColoredImageEffect.ColorY.B));
                _ColoredImageEffectInverted.ColorZ = Color.FromRgb((byte)(255 - _ColoredImageEffect.ColorZ.R), (byte)(255 - _ColoredImageEffect.ColorZ.G), (byte)(255 - _ColoredImageEffect.ColorZ.B));
            }
            else
            {
                byte c = Convert.ToByte(255f * 0.75f);
                _ColoredImageEffectInverted.ColorY = Color.FromRgb(c, 0, c);
                _ColoredImageEffectInverted.ColorX = Color.FromRgb(0, c, c);
                _ColoredImageEffectInverted.ColorZ = Color.FromRgb(c, c, 0);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
