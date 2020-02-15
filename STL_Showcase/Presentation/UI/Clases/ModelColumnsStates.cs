using STL_Showcase.Presentation.UI.Clases.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace STL_Showcase.Presentation.UI.Clases
{

    /// <summary>
    /// Manages the state of the main columns.
    /// </summary>
    public class ModelColumnsStates
    {

        bool[] modeEnabled;
        int modePowered = -1;

        ColumnDefinition[] columns;
        double[] defaultColumnSizes;
        double[] defaultColumnMinSizes;

        double animationDuration;

        public ModelColumnsStates(ColumnDefinition[] columns,
            double[] defaultColumnSizes,
            double[] defaultColumnMinSizes,
            double animationDuration)
        {
            this.columns = columns;
            modeEnabled = columns.Select(c => true).ToArray();

            this.defaultColumnSizes = defaultColumnSizes;
            this.defaultColumnMinSizes = defaultColumnMinSizes;

            this.animationDuration = animationDuration;
        }

        public bool IsColumnEnabled(int columnIndex)
        {
            return modeEnabled[columnIndex];
        }
        public bool IsColumnPowered(int columnIndex)
        {
            return modePowered == columnIndex;
        }
        public int GetColumnPowered()
        {
            return modePowered;
        }
        public enum ColumnState
        {
            Visibility = 0,
            Powered = 1,
            Reset = 2
        }

        public void SetNewState(int columnIndex, ColumnState stateAffected, bool useAnimation)
        {

            bool[] modeEnabledNew = new bool[modeEnabled.Length];
            int modePoweredNew = modePowered;
            modeEnabled.CopyTo(modeEnabledNew, 0);


            if (stateAffected == ColumnState.Visibility)
            { // Enable/Disable
                modeEnabledNew[columnIndex] = !modeEnabledNew[columnIndex];
                if (!modeEnabledNew[columnIndex] && modePoweredNew == columnIndex)
                    modePoweredNew = -1;
            }
            else if (stateAffected == ColumnState.Powered)
            { // Set/unset powered
                modePoweredNew = modePoweredNew == columnIndex ? -1 : columnIndex;

                if (!modeEnabled[columnIndex] && modePoweredNew == columnIndex)
                    modeEnabledNew[columnIndex] = true;
            }
            else
            {
                for (int i = 0; i < modeEnabledNew.Length; i++)
                {
                    modeEnabledNew[i] = true;
                }
                modePoweredNew = -1;
            }

            double totalDefaultSize = defaultColumnSizes.Sum();
            double columnsTotalSize = columns.Sum(c => c.Width.Value);

            // Normalize column sizes to fit totalSize.
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i].MinWidth = 0;
                if (columnsTotalSize == 0 || totalDefaultSize == 0)
                    columns[i].Width = new GridLength(0);
                else
                    columns[i].Width = new GridLength(totalDefaultSize / columnsTotalSize * columns[i].Width.Value, GridUnitType.Star);
            }

            double[] columnsNewSize = new double[3];
            for (int i = 0; i < columnsNewSize.Length; i++)
            {
                columnsNewSize[i] = modeEnabledNew[i] ? (defaultColumnSizes[i] * (modePoweredNew == i ? 3f : 1f)) : 0f;
            };

            // Animate columns whose enabled or powered state changes.
            for (int i = 0; i < columnsNewSize.Length; i++)
            {
                if (modeEnabled[i] || modeEnabledNew[i])
                {
                    var col = columns[i];
                    double newSize = columnsNewSize[i];

                    if (useAnimation)
                    {
                        GridLengthAnimation animation = new GridLengthAnimation();
                        animation.From = col.Width;
                        animation.To = new GridLength(newSize, GridUnitType.Star);
                        animation.Duration = new Duration(TimeSpan.FromMilliseconds(animationDuration));
                        animation.FillBehavior = FillBehavior.Stop; // Fixes GridSplitter not working after animation (first comment of https://stackoverflow.com/a/16844818/8577979)
                        animation.Completed += (s, _) =>
                        {
                            col.Width = new GridLength(newSize, GridUnitType.Star);
                        };

                        if (modeEnabled[i] != modeEnabledNew[i] && defaultColumnMinSizes[i] > 0f)
                        {
                            DoubleAnimation minSizeAnimation = new DoubleAnimation(
                                modeEnabledNew[i] ? defaultColumnMinSizes[i] : 0f,
                                new Duration(TimeSpan.FromMilliseconds(animationDuration)));
                            col.BeginAnimation(ColumnDefinition.MinWidthProperty, minSizeAnimation);
                        }
                        col.BeginAnimation(ColumnDefinition.WidthProperty, animation);
                    }
                    else
                    {
                        col.MinWidth = modeEnabledNew[i] ? defaultColumnMinSizes[i] : 0f;
                        col.Width = new GridLength(newSize, GridUnitType.Star);
                    }

                }
            }
            modeEnabled = modeEnabledNew;
            modePowered = modePoweredNew;

            //ColumnGridSplitterLeft.Visibility = (modeEnabled[0] && (modeEnabled[1] || modeEnabled[2])) ? Visibility.Visible : Visibility.Collapsed;
            //ColumnGridSplitterRight.Visibility = (modeEnabled[2] && modeEnabled[1]) ? Visibility.Visible : Visibility.Collapsed;

        }
    }
}
