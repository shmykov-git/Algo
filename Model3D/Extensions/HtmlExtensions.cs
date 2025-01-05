using System.Drawing;
using System;
using Model;
using System.Linq;
using Model.Extensions;
using System.IO;

namespace Model3D.Extensions;

public static class HtmlExtensions
{
    public static void CreateHtml(this Shape shape, string templateFilePath, string htmlFilePath)
    {
        var html = File.ReadAllText(templateFilePath);
        var get_scene_group = Get_get_scene_group(shape);
        html = html.Replace("function get_scene_group() {}", get_scene_group);
        File.WriteAllText(htmlFilePath, html);
    }

    private static string Get_get_scene_group(Shape shape, Color? defaultColor = default, bool wireframe = false, float cutFloat = 1E4f)
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

        string GetShapeScript(Shape mShape, int k)
        {
            var c = mShape.Materials?[0].Color ?? defaultColor ?? Color.LightGreen;
            var ps = mShape.Points3.Select(p => p.ToFloat()).SelectMany(p => new[] { p.x, p.y, p.z }).Select(CutFloat).ToArray();
            var indices = mShape.Triangles.ToArray();

            return $@"
  const vertices_{k} = new Float32Array([{ps.SJoin(",")}])
  const indices_{k} = new Uint16Array([{indices.SJoin(",")}])
  const geometry_{k} = new THREE.BufferGeometry()
  geometry_{k}.setAttribute('position', new THREE.BufferAttribute(vertices_{k}, 3))
  geometry_{k}.setIndex(new THREE.BufferAttribute(indices_{k}, 1))
  geometry_{k}.computeVertexNormals()
  const material_{k} = new THREE.MeshBasicMaterial({{color: {GetColorStr(c)}, transparent: true, opacity: {((float)c.A) / byte.MaxValue}, wireframe: {wireframe.ToString().ToLower()} }})
  const mesh_{k} = new THREE.Mesh(geometry_{k}, material_{k})
  group.add(mesh_{k})
";
        }

        var shapeScripts = shape.SplitByMaterial().Select(GetShapeScript);

        var script = $@"
function get_scene_group() {{
  const group = new THREE.Group()
{shapeScripts.SJoin("")}
  return group
}}
";

        return script;
    }
}
