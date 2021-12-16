using Aspose.ThreeD;
using System.Collections.Generic;
using System.IO;

namespace View3D
{
    class Settings
    {
        //public FileFormat Format => FileFormat.FBX7700Binary;
        public FileFormat Format => FileFormat.GLTF2_Binary;

        public string FileName => $"scene3d.{exts[Format]}";

        private static Dictionary<FileFormat, string> exts = new Dictionary<FileFormat, string>
        {
            { FileFormat.FBX7700Binary, "fbx" },
            { FileFormat.GLTF2_Binary, "glb" },
            { FileFormat.STLASCII, "stl" },
            { FileFormat.FBX7400ASCII, "fbx" },
            { FileFormat.WavefrontOBJ, "obj" },
        };

        private string OutputDirectory => Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\Scene");
        private string InputDirectory => Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\Content");
        public string FullFileName => Path.Combine(OutputDirectory, FileName);
        public bool AddNormalsWhenNoMaterial => true;
        public string GetContentFileName(string fileName) => Path.Combine(InputDirectory, fileName);
    }
}
