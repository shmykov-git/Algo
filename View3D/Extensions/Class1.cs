using System.Linq;
using Aspose.ThreeD.Utilities;

namespace View3D.Extensions;

public static class VectorExtensions
{
    public static Vector4 ToAspose(this Model3D.AsposeModel.Vector4 a) => new Vector4(a.x, a.y, a.z, a.w);
    public static Vector4[] ToAspose(this Model3D.AsposeModel.Vector4[] vectors) => vectors.Select(a => a.ToAspose()).ToArray();
}
