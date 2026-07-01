using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BamTest
{
    public class TestAssemblyDiscovery
    {
        /// <summary>
        /// Scan a directory for pre-built test assemblies. Each test project emits both a
        /// managed <c>*tests.dll</c> and an apphost <c>*tests.exe</c>; only the managed
        /// <c>.dll</c> is meant to be loaded and run as a test assembly. Returning both
        /// causes every assembly to be discovered — and executed — twice, where the
        /// apphost pass finds no tests and exits non-zero, producing spurious
        /// "0 passed, 0 failed" failures that make the overall run report failure even
        /// when all real tests pass. This collapses each assembly to a single entry,
        /// preferring the <c>.dll</c> and falling back to the <c>.exe</c> only when no
        /// sibling <c>.dll</c> exists.
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

            // Key by directory + base file name so a .dll and its sibling apphost .exe
            // collapse to one entry. Add .dll files first (preferred); an .exe is only
            // kept when there is no sibling .dll for that assembly.
            var byAssembly = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var dll in dlls)
            {
                string key = Path.Combine(Path.GetDirectoryName(dll) ?? string.Empty, Path.GetFileNameWithoutExtension(dll));
                byAssembly[key] = dll;
            }
            foreach (var exe in exes)
            {
                string key = Path.Combine(Path.GetDirectoryName(exe) ?? string.Empty, Path.GetFileNameWithoutExtension(exe));
                if (!byAssembly.ContainsKey(key))
                {
                    byAssembly[key] = exe;
                }
            }

            return byAssembly.Values
                .OrderBy(p => p)
                .ToList();
        }
    }
}
