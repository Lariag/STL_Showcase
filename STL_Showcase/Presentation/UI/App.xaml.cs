﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
        static App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
        }

        /// <summary>
        /// Copied from: https://stackoverflow.com/a/27891551
        /// 
        /// Tells the program that the Assembly its Seeking is located in the Embedded resources By using the
        /// <see cref="Assembly.GetManifestResourceNames"/> Function To get All the Resources
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <remarks>Note that this event won't fire if the dll is in the same folder as the application (sometimes)</remarks>
        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            try
            {
                //gets the main Assembly
                var parentAssembly = Assembly.GetExecutingAssembly();
                //args.Name will be something like this
                //[ MahApps.Metro, Version=1.1.3.81, Culture=en-US, PublicKeyToken=null ]
                //so we take the name of the Assembly (MahApps.Metro) then add (.dll) to it
                var finalname = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";
                //here we search the resources for our dll and get the first match
                var ResourcesList = parentAssembly.GetManifestResourceNames();
                string OurResourceName = null;
                //(you can replace this with a LINQ extension like [Find] or [First])
                for (int i = 0; i <= ResourcesList.Length - 1; i++)
                {
                    var name = ResourcesList[i];
                    if (name.EndsWith(finalname))
                    {
                        //Get the name then close the loop to get the first occuring value
                        OurResourceName = name;
                        break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(OurResourceName))
                {
                    //get a stream representing our resource then load it as bytes
                    using (Stream stream = parentAssembly.GetManifestResourceStream(OurResourceName))
                    {
                        //in vb.net use [ New Byte(stream.Length - 1) ]
                        //in c#.net use [ new byte[stream.Length]; ]
                        byte[] block = new byte[stream.Length];
                        stream.Read(block, 0, block.Length);
                        return Assembly.Load(block);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}