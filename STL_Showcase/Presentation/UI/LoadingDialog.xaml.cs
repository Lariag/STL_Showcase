using ModernWpf.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STL_Showcase.Presentation.UI
{
    /// <summary>
    /// Interaction logic for LoadingDialog.xaml
    /// </summary>
    public partial class LoadingDialog
    {
        private Action CancelAction;
        private bool ShouldClose = false;
        public LoadingDialog(string message, string title, string CancelButton = "", Action CancelAction = null)
        {
            InitializeComponent();
            this.Title = title;
            this.Message.Text = message;
            this.PrimaryButtonText = "";
            this.SecondaryButtonText = "";
            this.CloseButtonText = CancelButton;
            this.CancelAction = CancelAction;

            if (ShouldClose)
                this.Hide();
        }

        private void dialog_CloseButtonClick(ModernWpf.Controls.ContentDialog sender, ModernWpf.Controls.ContentDialogButtonClickEventArgs args)
        {
            CancelAction?.Invoke();
        }

        public void CloseDialog()
        {
            if (ShouldClose) return;

            this.ShouldClose = true;
            if (this.IsInitialized)
                this.Hide();
        }
    }
}
