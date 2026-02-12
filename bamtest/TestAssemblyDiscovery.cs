using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BamTest
{
    public class TestAssemblyDiscovery
    {
        /// <summary>
        /// Scan a directory for pre-built test assemblies (*tests.dll and *tests.exe).
        /// </summary>
        public List<string> FromDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Directory not found: {directoryPath}");
                return new List<string>();
            }

            var dlls = Directory.GetFiles(directoryPath, "*tests.dll", SearchOption.AllDirectories);
            var exes = Directory.GetFiles(directoryPath, "*tests.exe", SearchOption.AllDirectories);

            return dlls.Concat(exes)
                .OrderBy(p => p)
                .ToList();
        }
    }
}
