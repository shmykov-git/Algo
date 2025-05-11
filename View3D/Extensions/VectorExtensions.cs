using System.Linq;
using Model3D;
using ApQuaternion = Aspose.ThreeD.Utilities.Quaternion;
using ApVector3 = Aspose.ThreeD.Utilities.Vector3;
using ApVector4 = Aspose.ThreeD.Utilities.Vector4;

namespace View3D.Extensions;

public static class VectorExtensions
{
    public static ApVector4 ToAspose(this Vector4 a) => new ApVector4(a.x, a.y, a.z, a.w);
    public static ApVector4[] ToAspose(this Vector4[] vectors) => vectors.Select(a => a.ToAspose()).ToArray();

    public static ApVector3 ToAspose(this Vector3 a) => new ApVector3(a.x, a.y, a.z);
    public static Vector3 ToModel(this ApVector3 a) => new Vector3(a.X, a.Y, a.Z);

    public static ApQuaternion ToAspose(this Quaternion q) => new ApQuaternion(q.x, q.y, q.z, q.w);
    public static Quaternion ToModel(this ApQuaternion q) => new Quaternion(q.X, q.Y, q.Z, q.W);
}
