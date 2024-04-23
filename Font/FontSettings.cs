using Aspose.ThreeD;
using Model.Interfaces;

namespace Font;

public class FontSettings : IDirSettings
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

    public double MetallicFactor => 0.7; // 0 - пластик, 1 - метал (не блестит)

    public string OutputDirectory => Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\View3D\Scene");
    public string InputDirectory => Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\View3D\Content");
    public string FullFileName => Path.Combine(OutputDirectory, FileName);
    public bool AddNormalsWhenNoMaterial => false;
    public string GetContentFileName(string fileName) => Path.Combine(InputDirectory, fileName);
}
