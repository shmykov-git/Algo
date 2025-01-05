using System.Drawing;
using System;
using Model;
using System.Linq;
using Model.Extensions;
using System.IO;
using System.Collections.Generic;
using Meta;
using Model3D.Libraries;

namespace Model3D.Extensions;

public static class HtmlExtensions
{
    public static void CreateShapedHtml(this IEnumerable<Shape> shapes, string templateFilePath, string htmlFilePath)
    {
        var html = File.ReadAllText(templateFilePath);
        var get_scene_meshes = shapes.Select((shape, n) => Get_get_shape_n_mesh(shape, n)).ToArray();
        var get_scene_group = Get_get_scene_group(get_scene_meshes);
        html = html.Replace("// <generated vars/>", Get_generated_vars());
        html = html.Replace("function get_scene_group() { }", get_scene_group);
        File.WriteAllText(htmlFilePath, html);
    }

    public static void CreateHtml(this Shape shape, string templateFilePath, string htmlFilePath) =>
        new[] { shape }.CreateShapedHtml(templateFilePath, htmlFilePath);

    private static string Get_generated_vars() => @"
        const scene_meshes = []
";

    private static string Get_get_scene_group(string[] shapeMeshScripts)
    {
        return $@"
function get_scene_group() {{
  const scene_group = new THREE.Group()
  {(shapeMeshScripts).Index().Select(n => $"  scene_meshes.push(get_shape_{n}_mesh())").SJoin("\r\n")}
  scene_meshes.forEach(g => scene_group.add(g))
  return scene_group
}}
{shapeMeshScripts.SJoin("\r\n")}
";
    }

    private static string Get_get_shape_n_mesh(Shape shape, int n, Color? defaultColor = default, bool wireframe = false, float cutFloat = 1E4f)
    {
        string GetColorStr(Color c) => $"0x{c.R:X2}{c.G:X2}{c.B:X2}";
        string CutFloat(float f)
        {
            var s = (MathF.Round(f * cutFloat) / cutFloat).ToString();

            if (s == "0" || s == "-0")
                return "0";

            if (s.StartsWith("0"))
                return s[1..];

            if (s.StartsWith("-0"))
                return $"-{s[2..]}";

            return s;
        }

        List<Material> materials;
        (int mI, int[] ts)[] materialIndices;
        var hasMaterial = shape.Materials?.Any(m => m != null) ?? false;

        if (hasMaterial)
        {
            var mIndices = shape.TrianglesWithMaterials;
            mIndices.ForEach(m =>
            {
                if (m.m == null)
                    m.m = Materials.GetByColor(defaultColor ?? Color.Black);
            });
            materials = mIndices.Select(m => m.m).ToList();
            materialIndices = mIndices.Select(m => (mI: materials.IndexOf(m.m), m.ts)).OrderBy(v=>v.mI).ToArray();
        }
        else
        {
            var nomIndices = shape.Triangles.ToArray();
            materials = new[] { Materials.GetByColor(defaultColor ?? Color.Black) }.ToList();
            materialIndices = [(0, nomIndices)];
        }

        var ps = shape.Points3.Select(p => p.ToFloat()).SelectMany(p => new[] { p.x, p.y, p.z }).Select(CutFloat).ToArray();
        var indices = materialIndices.SelectMany(m=>m.ts).ToArray();
            
        string GetMaterial(Color c)
        {
            //return $"new THREE.MeshBasicMaterial({{color: {GetColorStr(c)}, transparent: true, opacity: {((float)c.A) / byte.MaxValue}, wireframe: {wireframe.ToString().ToLower()} }})";
            return $"new THREE.MeshStandardMaterial({{color: {GetColorStr(c)}, flatShading: false }})";
        }

        return $@"  
function get_shape_{n}_mesh() {{
  const materials = []
  {materials.Select(m => $"  materials.push({GetMaterial(m.Color)})").SJoin("\r\n")}
  const vertices = new Float32Array([{ps.SJoin(",")}])
  const materialIndices = [{materialIndices.Select(m => $"[{m.mI},[{m.ts.SJoin(",")}]]").SJoin(",")}]
  const indices = new Uint16Array(materialIndices.flatMap(m => m[1]))
  let sum = 0
  const materialGroups = materialIndices.map(m => [(()=>{{let r = sum; sum += m[1].length; return r;}})(), m[1].length, m[0]])
  const geometry = new THREE.BufferGeometry()
  geometry.setAttribute('position', new THREE.BufferAttribute(vertices, 3))
  geometry.setIndex(new THREE.BufferAttribute(indices, 1))
  materialGroups.forEach(g => geometry.addGroup(g[0], g[1], g[2]))  
  geometry.computeVertexNormals()
  const mesh = new THREE.Mesh(geometry, materials);
  return mesh
}}
";
    }
}
