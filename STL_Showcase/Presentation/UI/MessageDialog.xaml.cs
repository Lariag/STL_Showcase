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
    /// Interaction logic for MessageDialog.xaml
    /// </summary>
    public partial class MessageDialog
    {
        public MessageDialog(string message, string title, string YesButton, string NoButton, string CancelButton)
        {
            InitializeComponent();
            this.Title = title;
            this.Message.Text = message;
            this.PrimaryButtonText = YesButton;
            this.SecondaryButtonText = NoButton;
            this.CloseButtonText = CancelButton;
        }

    }
}
