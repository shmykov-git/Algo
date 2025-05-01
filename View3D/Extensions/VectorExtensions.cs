using System.Linq;
using Aspose.ThreeD.Utilities;

namespace View3D.Extensions;

public static class VectorExtensions
{
    public static Vector4 ToAspose(this Model3D.AsposeModel.Vector4 a) => new Vector4(a.x, a.y, a.z, a.w);
    public static Vector4[] ToAspose(this Model3D.AsposeModel.Vector4[] vectors) => vectors.Select(a => a.ToAspose()).ToArray();

    public static Vector3 ToAspose(this Model3D.AsposeModel.Vector3 a) => new Vector3(a.x, a.y, a.z);
    public static Model3D.AsposeModel.Vector3 ToModel(this Vector3 a) => new Model3D.AsposeModel.Vector3(a.X, a.Y, a.Z);

    public static Quaternion ToAspose(this Model3D.AsposeModel.Quaternion q) => new Quaternion(q.x, q.y, q.z, q.w);
    public static Model3D.AsposeModel.Quaternion ToModel(this Quaternion q) => new Model3D.AsposeModel.Quaternion(q.X, q.Y, q.Z, q.W);
}
