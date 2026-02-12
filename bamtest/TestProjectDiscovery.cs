using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BamTest
{
    public class TestProjectDiscovery
    {
        private static readonly Regex SlnProjectRegex = new Regex(
            @"Project\("".+""\)\s*=\s*"".+""\s*,\s*""(?<path>[^""]+\.tests\.csproj)""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Discover test projects from a solution file by parsing .sln for project entries matching *.tests.csproj.
        /// </summary>
        public List<string> FromSolution(string slnPath)
        {
            if (!File.Exists(slnPath))
            {
                Console.WriteLine($"Solution file not found: {slnPath}");
                return new List<string>();
            }

            string? slnDir = Path.GetDirectoryName(Path.GetFullPath(slnPath));
            string slnContent = File.ReadAllText(slnPath);
            var results = new List<string>();

            foreach (Match match in SlnProjectRegex.Matches(slnContent))
            {
                string relativePath = match.Groups["path"].Value.Replace('\\', Path.DirectorySeparatorChar);
                string fullPath = Path.GetFullPath(Path.Combine(slnDir ?? ".", relativePath));
                if (File.Exists(fullPath))
                {
                    results.Add(fullPath);
                }
            }

            return results.OrderBy(p => p).ToList();
        }

        /// <summary>
        /// Discover test projects by recursively searching a directory for *.tests.csproj files.
        /// </summary>
        public List<string> FromDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Directory not found: {directoryPath}");
                return new List<string>();
            }

            return Directory.GetFiles(directoryPath, "*.tests.csproj", SearchOption.AllDirectories)
                .OrderBy(p => p)
                .ToList();
        }

        /// <summary>
        /// Auto-discover test projects: look for a .sln in the given directory, or fall back to directory scan.
        /// </summary>
        public List<string> AutoDiscover(string? baseDirectory = null)
        {
            baseDirectory = baseDirectory ?? Directory.GetCurrentDirectory();

            string[] slnFiles = Directory.GetFiles(baseDirectory, "*.sln", SearchOption.TopDirectoryOnly);
            if (slnFiles.Length > 0)
            {
                Console.WriteLine($"Found solution: {slnFiles[0]}");
                return FromSolution(slnFiles[0]);
            }

            Console.WriteLine($"No solution file found, scanning directory: {baseDirectory}");
            return FromDirectory(baseDirectory);
        }
    }
}
