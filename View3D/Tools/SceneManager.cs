using Aspose.ThreeD;
using Aspose.ThreeD.Entities;

namespace View3D.Tools
{
    class SceneManager
    {
        public Scene CreateScene(Model.Shape shape)
        {
            Scene scene = new Scene();
            Node node = scene.RootNode.CreateChildNode("main");

            node.Entity = CreateMesh(shape);

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
