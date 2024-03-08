namespace Model3D.Voxels;

public class VapeWorldOptions
{
    public double VoxelSize { get; set; }
    public double InteractionMult { get; set; }
    public double InactiveSpeed { get; set; }

    //<debug>
    public int stepFuncExecCount; 
    //</debug>
}
