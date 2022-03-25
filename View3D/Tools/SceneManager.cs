using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Shading;
using Aspose.ThreeD.Utilities;
using Model3D.Extensions;
using Model3D.Libraries;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace View3D.Tools
{
    class SceneManager
    {
        private readonly Settings settings;

        public SceneManager(Settings settings)
        {
            this.settings = settings;
        }

        public Aspose.ThreeD.Scene CreateScene(Model.Shape shape)
        {
            Aspose.ThreeD.Scene scene = new Aspose.ThreeD.Scene();
            
            if (shape.Materials == null)
            {
                AddMaterialNode(scene, shape, defaultMaterial, settings.AddNormalsWhenNoMaterial);
            }
            else
            {
                var mShapes = shape.SplitByMaterial();
                Debug.WriteLine($"Number of materials: {mShapes.Length}");

                if (mShapes.Length > Materials.MaxMaterialsCount)
                    throw new ApplicationException($"Too much materials {mShapes.Length} > {Materials.MaxMaterialsCount}");

                foreach (var mShape in mShapes)
                    AddMaterialNode(scene, mShape, mShape.Materials?[0], false);
            }

            return scene;
        }

        private void AddMaterialNode(Aspose.ThreeD.Scene scene, Model.Shape shape, Model.Material material, bool addNormals)
        {
            Node main = scene.RootNode.CreateChildNode();
            main.Entity = CreateMesh(shape, addNormals);

            var m = material ?? defaultMaterial;
            //var t = new Texture(@"C:\Users\SHMYKOV\OneDrive\Изображения\паркет.png");

            main.Material = new PbrMaterial()
            {
                MetallicFactor = settings.MetallicFactor,
                RoughnessFactor = 0,
                //OcclusionTexture = t,
                EmissiveColor = new Vector3(m.Color),
            };

            //main.Material = new PhongMaterial()
            //{
            //    //Shininess = 0,
            //    //ReflectionFactor = 0,
            //    //EmissiveColor = new Vector3(m.Color)
            //    DiffuseColor = new Vector3(m.Color)
            //};
        }

        private Mesh CreateMesh(Model.Shape shape, bool addNormals)
        {
            var mesh = new Mesh();
            mesh.ControlPoints.AddRange(shape.Points);

            if (addNormals)
            {
                var normals = PolygonModifier.GenerateNormal(mesh);
                mesh.VertexElements.Add(normals);

                var uv = PolygonModifier.GenerateUV(mesh);
                mesh.VertexElements.Add(uv);

                //var mats = (VertexElementMaterial)mesh.CreateElement(VertexElementType.Material);
                //mats.MappingMode = MappingMode.Polygon;
                
            }

            foreach (var convex in shape.Convexes.Where(c=>c.Length>=3))
            {
                mesh.CreatePolygon(convex);
            }

            return mesh;
        }

        private static Model.Material defaultMaterial = new Model.Material { Color = Color.Black };
    }
}
