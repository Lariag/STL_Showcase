using STL_Showcase.Logic.Localization;
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
    public partial class AboutDialog
    {
        public AboutDialog()
        {
            InitializeComponent();
            this.Title = ""; // Loc.GetText("About"); // Replaced with app logo/name.
            this.Message.Text = Loc.GetText("About_MainDescription");
            tbGithubLink.Text = $"{Loc.GetText("CodePlatformName")}: ";
            GithubLink.Content = "https://github.com/Alriac/STL_Showcase";
            tbIssuesLink.Text = $"{Loc.GetText("CodePlatformIssues")}: ";
            GithubLink.NavigateUri = new Uri("https://github.com/Alriac/STL_Showcase");
            IssuesLink.Content = "https://github.com/Alriac/STL_Showcase/issues";
            IssuesLink.NavigateUri = new Uri("https://github.com/Alriac/STL_Showcase/issues");

            this.tbVersion.Text = $"{Loc.GetText("Version")}: ";
            this.Version.Text = App.AppVersion;

            this.PrimaryButtonText = Loc.GetText("OK");
            this.SecondaryButtonText = string.Empty;
            this.CloseButtonText = string.Empty;
        }

    }
}
