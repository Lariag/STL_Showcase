using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace STL_Showcase.Presentation.UI.Clases
{
    public class ShaderEffectInvert : ShaderEffect
    {
        private static readonly PixelShader _shader =
           //new PixelShader { UriSource = new Uri("pack://application:,,,/Presentation/UI/Styles/Shaders/NormalAsColors.ps") };
           new PixelShader { UriSource = new Uri("pack://application:,,,/Presentation/UI/Styles/Shaders/ShaderInvertColor.ps") };

        public ShaderEffectInvert()
        {
            PixelShader = _shader;
            UpdateShaderValue(InputProperty);
        }

        public Brush Input {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(ShaderEffectInvert), 0);
    }

    public class ShaderEffectNormalColoring : ShaderEffect
    {
        private static readonly PixelShader _shader =
           new PixelShader { UriSource = new Uri("pack://application:,,,/Presentation/UI/Styles/Shaders/NormalAsColors.ps") };

        public ShaderEffectNormalColoring()
        {
            PixelShader = _shader;
            UpdateShaderValue(InputProperty);
        }

        public Brush Input {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public Color ColorX {
            get { return (Color)GetValue(ColorXProperty); }
            set { SetValue(ColorXProperty, value); }
        }

        public Color ColorY {
            get { return (Color)GetValue(ColorYProperty); }
            set { SetValue(ColorYProperty, value); }
        }

        public Color ColorZ {
            get { return (Color)GetValue(ColorZProperty); }
            set { SetValue(ColorZProperty, value); }
        }

        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(ShaderEffectNormalColoring), 0);

        public static readonly DependencyProperty ColorXProperty =
            DependencyProperty.Register("ColorX", typeof(Color), typeof(ShaderEffectNormalColoring), new UIPropertyMetadata(Colors.Gray, PixelShaderConstantCallback(0)));

        public static readonly DependencyProperty ColorYProperty =
            DependencyProperty.Register("ColorY", typeof(Color), typeof(ShaderEffectNormalColoring), new UIPropertyMetadata(Colors.Gray, PixelShaderConstantCallback(1)));

        public static readonly DependencyProperty ColorZProperty =
            DependencyProperty.Register("ColorZ", typeof(Color), typeof(ShaderEffectNormalColoring), new UIPropertyMetadata(Colors.Gray, PixelShaderConstantCallback(2)));

    }
}
