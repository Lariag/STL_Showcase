using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace STL_Showcase.Presentation.UI.Clases.Utility
{
    /// <summary>
    /// Animates a GridLenght.
    /// Source from: https://www.codeproject.com/Articles/18379/WPF-Tutorial-Part-2-Writing-a-custom-animation-cla
    /// </summary>
    internal class GridLengthAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType {
            get
            {
                return typeof(GridLength);
            }
        }

        protected override System.Windows.Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }
        static GridLengthAnimation()
        {
            FromProperty = DependencyProperty.Register("From", typeof(GridLength),
                typeof(GridLengthAnimation));

            ToProperty = DependencyProperty.Register("To", typeof(GridLength),
                typeof(GridLengthAnimation));
        }
        public static readonly DependencyProperty FromProperty;
        public GridLength From {
            get
            {
                return (GridLength)GetValue(GridLengthAnimation.FromProperty);
            }
            set
            {
                SetValue(GridLengthAnimation.FromProperty, value);
            }
        }
        public static readonly DependencyProperty ToProperty;
        public GridLength To {
            get
            {
                return (GridLength)GetValue(GridLengthAnimation.ToProperty);
            }
            set
            {
                SetValue(GridLengthAnimation.ToProperty, value);
            }
        }
        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            double fromVal = ((GridLength)GetValue(GridLengthAnimation.FromProperty)).Value;
            double toVal = ((GridLength)GetValue(GridLengthAnimation.ToProperty)).Value;

            if (fromVal > toVal)
            {
                return new GridLength((1 - animationClock.CurrentProgress.Value) *
                    (fromVal - toVal) + toVal, GridUnitType.Star);
            }
            else
            {
                return new GridLength(animationClock.CurrentProgress.Value *
                    (toVal - fromVal) + fromVal, GridUnitType.Star);
            }
        }
    }
}
