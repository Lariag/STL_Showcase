using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STL_Showcase.Presentation.UI.Clases
{
    /// <summary>
    /// Directory processing progress bar.
    /// </summary>
    public class ModelProgressBarData : INotifyPropertyChanged
    {
        private bool _ReversedMode;
        public bool ReversedMode {
            get { return _ReversedMode; }
            set
            {
                if (value != _ReversedMode)
                    this.CurrentProgress = 100 - this._CurrentProgress;
                this._ReversedMode = value;
            }
        }

        private bool _IsLoading;
        public bool IsLoading { get { return _IsLoading; } set { _IsLoading = value; NotifyPropertyChanged(nameof(IsLoading)); } }
        private int _MaxProgress { get; set; }
        public int MaxProgress { get { return _MaxProgress; } set { _MaxProgress = value; NotifyPropertyChanged(nameof(MaxProgress)); } }
        private int _CurrentProgress;
        public int CurrentProgress {
            get { return _CurrentProgress; }
            set
            {
                this._CurrentProgress = value;
                CurrentProgressPercent = (int)Math.Round(((float)CurrentProgress / (float)MaxProgress) * 100f);
                NotifyPropertyChanged(nameof(CurrentProgress));
            }
        }
        private int _CurrentProgressPercent;
        public int CurrentProgressPercent { get { return ReversedMode ? 100 - _CurrentProgressPercent : _CurrentProgressPercent; } private set { _CurrentProgressPercent = value; NotifyPropertyChanged(nameof(CurrentProgressPercent)); } }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

    }
}
