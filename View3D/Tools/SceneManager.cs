using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Shading;
using Aspose.ThreeD.Utilities;
using System.Drawing;

namespace View3D.Tools
{
    class SceneManager
    {
        public Scene CreateScene(Model.Shape shape)
        {
            Scene scene = new Scene();
            
            Node main = scene.RootNode.CreateChildNode("main");
            main.Entity = CreateMesh(shape);

            PbrMaterial material = new PbrMaterial();
            // an almost metal material
            material.MetallicFactor = 0.9;
            // material surface is very rough
            material.RoughnessFactor = 0.9;
            main.Material = material;

            Node light = scene.RootNode.CreateChildNode("light");
            light.Entity = new Light() 
            { 
                Color = new Vector3(Color.Green), 
                LightType = LightType.Area 
            };
            light.Transform.Translation = new Vector3(100, 200, 300);

            return scene;
        }

        private Mesh CreateMesh(Model.Shape shape)
        {
            var mesh = new Mesh();
            mesh.ControlPoints.AddRange(shape.Points);
            
            foreach (var convex in shape.Convexes)
            {
                mesh.CreatePolygon(convex);
            }

            return mesh;
        }
    }
}
