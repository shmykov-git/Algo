using Aspose.ThreeD;
using System.Collections.Generic;
using System.IO;

namespace View3D
{
    class Settings
    {
        public FileFormat Format => FileFormat.FBX7700ASCII;
        
        public string FileName => $"scene3d.{exts[Format]}";

        private static Dictionary<FileFormat, string> exts = new Dictionary<FileFormat, string>
        {
            { FileFormat.STLASCII, "stl" },
            { FileFormat.FBX7700Binary, "fbx" },
            { FileFormat.FBX7700ASCII, "fbx" }
        };

        private string OutputDirectory => Path.Combine(Directory.GetCurrentDirectory(), "../../../Scene");
        public string FullFileName => Path.Combine(OutputDirectory, FileName);
    }
}
