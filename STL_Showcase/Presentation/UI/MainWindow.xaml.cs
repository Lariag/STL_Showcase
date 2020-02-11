using ModernWpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STL_Showcase.Presentation.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Style InitialStyle;
        WindowState maximizedStatus;
        readonly Thickness defaultPageMargin;
        Page MainPage;

        public Action ClosingEvent;
        public MainWindow()
        {
            InitializeComponent();
            MainPage = new ModelShowcase(this);
            this.Pages.Content = MainPage;
            InitialStyle = this.Style;
            maximizedStatus = this.WindowState;
            defaultPageMargin = Pages.Margin;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.GetPosition(this).Y < 30)
                if (e.ClickCount == 2)
                {
                    AdjustWindowSize();
                }
                else
                {
                    Application.Current.MainWindow.DragMove();
                }
        }

        private void AdjustWindowSize()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Minimized) return;

            if (this.WindowState == WindowState.Normal)
            {
                Pages.Margin = defaultPageMargin;
            }
            else
            {
                Pages.Margin = new Thickness(defaultPageMargin.Left, defaultPageMargin.Top, defaultPageMargin.Right, SystemParameters.PrimaryScreenHeight - SystemParameters.MaximizedPrimaryScreenHeight + 15d);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ClosingEvent?.Invoke();
        }
    }

}
