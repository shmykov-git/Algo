using System.IO;

namespace View3D
{
    class Settings
    {
        public string FbxFileName => "scene3d.stl";

        private string OutputDirectory => Path.Combine(Directory.GetCurrentDirectory(), "../../../Scene");
        public string FbxFullFileName => Path.Combine(OutputDirectory, FbxFileName);
    }
}
