using MessagePack;
using System.Collections.Generic;

namespace View3D.Tools.Model;

[MessagePackObject]
public class Animate
{
    [Key("meshes")]
    public List<Mesh> meshes = new();

    [MessagePackObject]
    public class Mesh
    {
        [Key("points0")]
        public float[] points0 { get; set; }

        [Key("index")]
        public ushort[] index { get; set; }

        [Key("links")]
        public int[][] links { get; set; }

        [Key("moves")]
        public float[][] moves { get; set; }
    }
}
