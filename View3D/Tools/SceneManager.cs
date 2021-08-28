using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Shading;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model3D.Extensions;
using System.Drawing;
using System.Linq;

namespace View3D.Tools
{
    class SceneManager
    {
        public Scene CreateScene(Model.Shape shape)
        {
            Scene scene = new Scene();
            
            if (shape.Materials == null)
            {
                AddMaterialNode(scene, shape, defaultMaterial);
            }
            else
            {
                foreach (var mShape in shape.SplitByMaterial())
                    AddMaterialNode(scene, mShape, mShape.Materials?[0]);
            }

            return scene;
        }

        private void AddMaterialNode(Scene scene, Model.Shape shape, Model.Material material)
        {
            Node main = scene.RootNode.CreateChildNode();
            main.Entity = CreateMesh(shape);

            main.Material = new PbrMaterial()
            {
                MetallicFactor = 0.9,
                RoughnessFactor = 0.9,
                EmissiveColor = new Vector3(material.Color)
            };
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

        private static Model.Material defaultMaterial = new Model.Material { Color = Color.Black };
    }
}
