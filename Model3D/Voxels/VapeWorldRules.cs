using Model.Extensions;

namespace Model3D.Voxels;

public static class VapeWorldRules
{
    public static double MaterialForce(double power, double border, double x)
    {
        if (x < border)
            x = border;

        return power * (x - 1) * (x + 1) / x.Pow4();
    }

    public static double InteractionForce(double power, double border, double x)
    {
        if (x < border)
            x = border;

        return -power / x.Pow4();
    }
}
