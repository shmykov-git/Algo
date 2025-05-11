using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Shading;
using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model3D.Extensions;
using Model3D.Libraries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using View3D.Extensions;
using AsposeScene = Aspose.ThreeD.Scene;
using AsposeMaterial = Aspose.ThreeD.Shading.Material;
using Material = Model.Material;
using Shape = Model.Shape;
using View3D.Tools.Model;
using System.Data;
using Model.Libraries;

namespace View3D.Tools;

public class StaticSceneRender
{
    private readonly StaticSettings staticSettings;
    private readonly Dictionary<Material, AsposeMaterial> materials = new();

    public StaticSceneRender(StaticSettings staticSettings)
    {
        this.staticSettings = staticSettings;
    }

    private AsposeMaterial GetMaterial(Material m)
    {
        if (materials.TryGetValue(m, out var material))
            return material;

        material = new PbrMaterial()
        {
            MetallicFactor = staticSettings.MetallicFactor,
            //RoughnessFactor = 1,
            //OcclusionTexture = t,
            EmissiveColor = new Vector3(new Model3D.Vector4(m.Color).ToAspose()),
        };

        materials.Add(m, material);

        return material;
    }

    public (AsposeScene scene, Animate animate) CreateAnimatedScene(Shape[] frameShapes)
    {
        frameShapes.ForEach(fs => fs.CompositeShapes.ForEach(s =>
        {
            if (!s.HasSingleMaterial)
                throw new ApplicationException("Animate support only single material shapes");

            if (s.Convexes.Any(c => c.Length > 3))
            {
                var ss = s.TriangulateByFour();
                s.Points = ss.Points;
                s.Convexes = ss.Convexes;
            }

            if (s.Convexes.Any(c => c.Length < 3))
            {
                s.Convexes = s.Convexes.Where(c => c.Length == 3).ToArray();
            }
        }));

        List<Animate.Mesh> animateMeshes = new();

        Animate.Mesh GetAnimateMesh(int meshIndex, Shape[] frames)
        {
            var frame0 = frames[0];

            if (frame0.Convexes.Any(c => c.Length != 3))
                throw new ApplicationException("Animate must be triangulated, use shape.TriangulateByFour");

            var mesh = new Animate.Mesh
            {
                points0 = frame0.Points.SelectMany(p => new[] { (float)p.x, (float)p.y, (float)p.z }).ToArray(),
                index = frame0.Convexes.SelectMany(c => c).Select(i => (ushort)i).ToArray()
            };
            
            if (frames[0].HasMasterPoints)
            {
                mesh.moves = frames.Select(f => f.MasterPoints.Select((mp, i) => mp.point - frame0.MasterPoints[i].point).SelectMany(p => new[] { (float)p.x, (float)p.y, (float)p.z }).ToArray()).ToArray();
                mesh.links = frame0.MasterPoints.Select(mp => mp.links).ToArray();
            }
            else
            {
                mesh.links = [];
                mesh.moves = frames.Select(f => frame0.Points.Select((p0, i) => f.Points[i] - p0).SelectMany(p => new[] { (float)p.x, (float)p.y, (float)p.z }).ToArray()).ToArray();
            }

            for(var i = 0; i < mesh.moves.Length; i++)
            {
                if (mesh.moves[i].All(m => m.Abs() < Values.Epsilon9))
                    mesh.moves[i] = [];
            }

            return mesh;
        }

        var frameShapeEnumerators = frameShapes.Select(s => s.CompositeShapes.GetEnumerator()).ToArray();
        AsposeScene scene = new();
        var meshIndex = 0;

        while (frameShapeEnumerators.All(v => v.MoveNext()))
        {
            var frames = frameShapeEnumerators.Select(v => v.Current).ToArray();
            Node node = scene.RootNode.CreateChildNode();
            InitSingleMaterialAnimatedNode(node, frames);
            animateMeshes.Add(GetAnimateMesh(meshIndex, frames));
            meshIndex++;
        }
        
        return (scene, new Animate { meshes = animateMeshes });
    }

    private void InitSingleMaterialAnimatedNode(Node node, Shape[] frames)
    {
        var shape0 = frames[0];

        var mesh = new Mesh();
        node.Entity = mesh;
        mesh.ControlPoints.AddRange(shape0.Points.ToAspose());
        shape0.Convexes.ForEach(mesh.Polygons.Add);

        var m = shape0.Materials?[0] ?? defaultMaterial;
        node.Material = GetMaterial(m);

        if (staticSettings.AddNormalsWhenNoMaterial)
        {
            var normals = PolygonModifier.GenerateNormal(mesh);
            mesh.VertexElements.Add(normals);
        }
    }

    public AsposeScene CreateScene(Shape shape)
    {
        AsposeScene scene = new();

        foreach (var s in shape.CompositeMaterialShapes) 
        {
            Node node = scene.RootNode.CreateChildNode();
            InitSingleMaterialNode(node, s);
        }

        return scene;
    }

    private void InitSingleMaterialNode(Node node, Shape shape)
    {
        if (shape.Convexes.Any(c => c.Length < 3))
            throw new NotSupportedException("Convex length < 3");

        var mesh = new Mesh();
        mesh.ControlPoints.AddRange(shape.Points.ToAspose());
        shape.Convexes.ForEach(mesh.Polygons.Add);

        var m = shape.Materials?[0] ?? defaultMaterial;
        node.Material = GetMaterial(m);

        // не получается сделать несколько материалов в одном mesh
        //var (uniqueMaterials, bi, _) = shape.Materials.DistinctBi();
        //uniqueMaterials.ForEach(m => node.Materials.Add(GetMaterial(m)));

        //VertexElementMaterial materialElement = (VertexElementMaterial)mesh.CreateElement(VertexElementType.Material, MappingMode.Polygon, ReferenceMode.Index);
        //materialElement.Indices.AddRange(bi);
        //mesh.VertexElements.Add(materialElement);

        //node.Material = node.Materials[1];

        if (staticSettings.AddNormalsWhenNoMaterial)
        {
            var normals = PolygonModifier.GenerateNormal(mesh);
            mesh.VertexElements.Add(normals);
        }

        node.Entity = mesh;
    }

    public AsposeScene CreateScene1(Shape shape)
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

    private void AddMaterialNode(AsposeScene scene, Shape shape, Material material, bool addNormals)
    {
        Node main = scene.RootNode.CreateChildNode();
        main.Entity = CreateMesh(shape, false);

        var m = material ?? defaultMaterial;

        main.Material = GetMaterial(m);
    }

    private Mesh CreateMesh(Shape shape, bool addNormals)
    {
        var mesh = new Mesh();
        mesh.ControlPoints.AddRange(shape.Points.ToAspose());

        if (addNormals)
        {
            var normals = PolygonModifier.GenerateNormal(mesh);
            mesh.VertexElements.Add(normals);

            //var uv = PolygonModifier.GenerateUV(mesh);
            //mesh.VertexElements.Add(uv);

            //var mats = (VertexElementMaterial)mesh.CreateElement(VertexElementType.Material);
            //mats.MappingMode = MappingMode.Polygon;

        }

        foreach (var convex in shape.Convexes.Where(c=>c.Length>=3))
        {
            mesh.CreatePolygon(convex);
        }

        return mesh;
    }

    private static Material defaultMaterial = new Material { Color = Color.Black };
}
