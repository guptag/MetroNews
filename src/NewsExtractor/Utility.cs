using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace NewsExtractor
{
    public class Utility
    {
        public static string GetCurrentDirectory()
        {
            //http://www.markbarto.nl/blog/archives/2009/9/1/c-net-current-directory-for-windows-service

            // Getting the URI format returned of the executed assembly
            string cb = Assembly.GetExecutingAssembly().CodeBase;

            // The codebase will return as URI file:// and UnescapeDataString will remove this
            UriBuilder uri = new UriBuilder(cb);
            string path = Uri.UnescapeDataString(uri.Path);

            // Getting the correct current directory
            string currentDirectory = Path.GetDirectoryName(path);

            return currentDirectory;
        }
    }
}
