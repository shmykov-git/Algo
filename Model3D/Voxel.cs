using System;

namespace Model3D;

public struct Voxel
{
    public int i;
    public int j;
    public int k;

    public static Voxel zero = new Voxel(0, 0, 0);

    public Voxel(int i, int j, int k)
    {
        this.i = i;
        this.j = j;
        this.k = k;
    }

    public override string ToString() => $"({i}, {j}, {k})";

    public int len => Math.Abs(i) + Math.Abs(j) + Math.Abs(k);
    
    
    public static implicit operator Voxel((int i, int j, int k) v)
    {
        return new Voxel(v.i, v.j, v.k);
    }

    public static Voxel operator -(Voxel a)
    {
        return new Voxel(-a.i, -a.j, -a.k);
    }

    public static Voxel operator +(Voxel a)
    {
        return a;
    }

    public static Voxel operator -(Voxel a, Voxel b)
    {
        return new Voxel(a.i - b.i, a.j - b.j, a.k - b.k);
    }

    public static Voxel operator +(Voxel a, Voxel b)
    {
        return new Voxel(a.i + b.i, a.j + b.j, a.k + b.k);
    }

    public static Vector3 operator *(Voxel a, Vector3 b)
    {
        return new Vector3(a.i * b.x, a.j * b.y, a.k * b.z);
    }

    public static bool operator !=(Voxel a, Voxel b)
    {
        return !a.Equals(b);
    }

    public static bool operator ==(Voxel a, Voxel b)
    {
        return a.Equals(b);
    }
}
