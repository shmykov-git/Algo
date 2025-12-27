using Model.Interfaces;
using System;
using System.IO;
using System.Linq;

namespace Model3D.Tools
{
    public class ContentFinder
    {
        private readonly IDirSettings settings;

        private readonly string[] exes = { ".jpg", ".png" };

        public ContentFinder(IDirSettings settings)
        {
            this.settings = settings;
        }

        public string FindContentFileName(string name)
        {
            var allFiles = Directory.GetFiles(settings.InputDirectory);
            var files = allFiles.Where(f => exes.Any(exe => f.EndsWith(name + exe))).ToArray();

            if (files.Length == 0)
                throw new ArgumentException($"Cannot find content file {name}");

            if (files.Length > 1)
                Console.Write($"Found {files.Length} files for {name}!");

            return files.First();
        }
    }
}