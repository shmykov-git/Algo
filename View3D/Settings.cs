using System.IO;

namespace View3D
{
    class Settings
    {
        public string FbxFileName => "scene3d.fbx";
        public string FbxFullFileName => Path.Combine(Directory.GetCurrentDirectory(), FbxFileName);
    }
}
