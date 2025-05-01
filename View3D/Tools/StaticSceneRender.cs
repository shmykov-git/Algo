using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Shading;
using Aspose.ThreeD.Utilities;
using meta.Extensions;
using Model.Extensions;
using Model3D.Extensions;
using Model3D.Libraries;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using AsposeScene = Aspose.ThreeD.Scene;
using Shape = Model.Shape;

namespace View3D.Tools
{
    public class StaticSceneRender
    {
        private readonly StaticSettings staticSettings;

        public StaticSceneRender(StaticSettings staticSettings)
        {
            this.staticSettings = staticSettings;
        }

        public AsposeScene CreateScene(Shape[] shapes)
        {
            AsposeScene scene = new();

            foreach (var shape in shapes) 
            {
                Node shapeNode = scene.RootNode.CreateChildNode();
                InitNode(shapeNode, shape);
            }

            return scene;
        }

        private Material GetMaterial(Model.Material m)
        {
            return new PbrMaterial()
            {
                MetallicFactor = staticSettings.MetallicFactor,
                //RoughnessFactor = 1,
                //OcclusionTexture = t,
                EmissiveColor = new Vector3(m.Color),
            };
        }

        private void InitNode(Node node, Shape shape)
        {
            //node.Materials.
            var mesh = CreateMesh(shape, false);
            PolygonModifier.GenerateNormal(mesh);
            node.Entity = mesh;

            var (bi, filter) = shape.Materials.DistinctBi();

            foreach(var (m, i) in shape.Materials.Select((m,i)=>(m,i)))
            {
                if (filter[i])
                    node.Materials.Add(GetMaterial(m));
                

            }
        }

        public AsposeScene CreateScene(Shape shape)
        {
            AsposeScene scene = new AsposeScene();
            
            if (shape.Materials == null)
            {
                AddMaterialNode(scene, shape, defaultMaterial, staticSettings.AddNormalsWhenNoMaterial);
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

        private void AddMaterialNode(AsposeScene scene, Shape shape, Model.Material material, bool addNormals)
        {
            Node main = scene.RootNode.CreateChildNode();
            main.Entity = CreateMesh(shape, addNormals);

            var m = material ?? defaultMaterial;
            //var t = new Texture(@"C:\Users\SHMYKOV\OneDrive\Изображения\паркет.png");

            main.Material = new PbrMaterial()
            {
                MetallicFactor = staticSettings.MetallicFactor,
                //RoughnessFactor = 1,
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

        private Mesh CreateMesh(Shape shape, bool addNormals)
        {
            var mesh = new Mesh();
            mesh.ControlPoints.AddRange(shape.Points);

            if (addNormals)
            {
                //var normals = PolygonModifier.GenerateNormal(mesh);
                //mesh.VertexElements.Add(normals);

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
