namespace Model3D.Vapes;

public class VoxelEdge
{
    public VapeVoxel a;
    public VapeVoxel b;

    public VapeVoxel another(VapeVoxel v) => a == v ? b : a;
    public double materialPower => 0.5 * (a.material.power + b.material.power);
    public double materialPowerRadius => 0.5 * (a.material.powerRadius + b.material.powerRadius);
    public double materialDestroyRadius => 0.5 * (a.material.destroyRadius + b.material.destroyRadius);
    public double mass => 0.5 * (a.material.mass + b.material.mass);
}
