using Aspose.ThreeD;
using Model.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace View3D
{
    public class StaticSettings : IDirSettings
    {
        //public FileFormat Format => FileFormat.FBX7700Binary;
        public FileFormat Format => FileFormat.GLTF2_Binary;

        public string FileName3D => $"scene3d.{exts[Format]}";
        public string FileNameHtml => $"scene3d.html";
        public string FileNameAnimate => $"scene3d.animate";

        private static Dictionary<FileFormat, string> exts = new Dictionary<FileFormat, string>
        {
            { FileFormat.FBX7700Binary, "fbx" },
            { FileFormat.GLTF2_Binary, "glb" },
            { FileFormat.STLASCII, "stl" },
            { FileFormat.FBX7400ASCII, "fbx" },
            { FileFormat.WavefrontOBJ, "obj" },
        };

        public double MetallicFactor => 0.7; // 0 - пластик, 1 - метал (не блестит)

        public string OutputDirectory => Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\View3D\Scene");
        public string InputDirectory => Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\View3D\Content");
        public string InputHtmlDirectory => Path.Combine(Directory.GetCurrentDirectory(), @"HtmlTemplates");

        public string FullFileName3D => Path.Combine(OutputDirectory, FileName3D);
        public string FullFileNameHtml => Path.Combine(OutputDirectory, FileNameHtml);
        public string FullFileNameAnimate => Path.Combine(OutputDirectory, FileNameAnimate);
        public bool AddNormalsWhenNoMaterial => false;
        public double TimeFrame = 0.04; // 25 кадров в секунду
        public string GetContentFileName(string fileName) => Path.Combine(InputDirectory, fileName);
    }
}
