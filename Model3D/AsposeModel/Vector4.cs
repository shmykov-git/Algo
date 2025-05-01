namespace Model3D.AsposeModel;

public struct Vector4
{
    public double x;
    public double y;
    public double z;
    public double w;

    public Vector4(double x, double y, double z, double w = 1.0)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public void Set(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = 1.0;
    }

    public void Set(double x, double y, double z, double w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public override string ToString()
    {
        return $"Vector4({x}, {y}, {z}, {w})";
    }
}
