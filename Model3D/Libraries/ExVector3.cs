using Model3D.AsposeModel;

namespace Model3D.Libraries;

public static class ExVector3
{
    public static readonly Vector3 Zero = Vector3.Origin;
    public static readonly Vector3 One = new Vector3(1, 1, 1);
    public static readonly Vector3 XyzAxis = new Vector3(1, 1, 1).Normalize();
    public static readonly Vector3 XyAxis = new Vector3(1, 1, 0).Normalize();
    public static readonly Vector3 XzAxis = new Vector3(1, 0, 1).Normalize();
    public static readonly Vector3 YzAxis = new Vector3(0, 1, 1).Normalize();
}
