using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STL_Showcase.Shared.Util
{
    public class UtilMethods
    {

        public static string GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// Enumerates all files while avoiding UnauthorizedAccessException.
        /// Source: https://stackoverflow.com/a/5957525/8577979
        /// </summary>
        /// <param name="path"></param>
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOpt, CancellationToken token = default(CancellationToken))
        {
            if (token.IsCancellationRequested)
                return Enumerable.Empty<string>();
            try
            {
                var dirFiles = Enumerable.Empty<string>();
                if (searchOpt == SearchOption.AllDirectories)
                {
                    dirFiles = Directory.EnumerateDirectories(path)
                                        .SelectMany(x => EnumerateFiles(x, searchPattern, searchOpt));
                }
                return dirFiles.Concat(Directory.EnumerateFiles(path, searchPattern));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Enumerable.Empty<string>();
            }
        }
    }
}
