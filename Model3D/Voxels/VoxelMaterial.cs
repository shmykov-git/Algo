namespace Model3D.Voxels;

public class VoxelMaterial
{
    public double mass;
    public double power;
    public double powerRadius;
    public double destroyRadius;
    public double damping;

    public VoxelInteraction interaction;
}

public class VoxelInteraction
{
    public double power;
    public double powerRadius;
}
