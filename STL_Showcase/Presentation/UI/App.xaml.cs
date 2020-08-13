using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace STL_Showcase.Presentation.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#if X64
        public const string AppVersion = "v0.5.0 x64 [BETA]";
#else
        public const string AppVersion = "v0.5.0 x32 [BETA]";
#endif
    }
}