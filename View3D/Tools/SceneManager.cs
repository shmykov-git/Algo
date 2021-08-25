using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Shading;
using Aspose.ThreeD.Utilities;
using Model;

namespace View3D.Tools
{
    class SceneManager
    {
        public Scene CreateScene(ShapeView[] views)
        {
            Scene scene = new Scene();
            
            foreach(var view in views)
            {
                Node main = scene.RootNode.CreateChildNode();
                main.Entity = CreateMesh(view.Shape);

                main.Material = new PbrMaterial()
                {
                    MetallicFactor = 0.9,
                    RoughnessFactor = 0.9,
                    EmissiveColor = new Vector3(view.Color)
                };
            }

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
