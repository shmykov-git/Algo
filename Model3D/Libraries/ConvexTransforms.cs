using System;

namespace Model3D.Libraries;


public delegate Vector3 ConvexTransformFunc(int i, int j, Vector3 p);

public static class ConvexTransforms
{
    public static ConvexTransformFunc Hedgehog(Func<Vector3, Vector3> topFn) => (i, j, p) => (i + j) % 2 == 0 ? topFn(p) : p;
    public static ConvexTransformFunc Hedgehog1(Func<Vector3, Vector3> topFn) => (i, j, p) => (i + j + 1) % 2 == 0 ? topFn(p) : p;
}
