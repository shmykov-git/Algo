namespace AI.Model;

public class NOptions
{
    public int Seed { get; set; } = 0;
    public double ShaffleFactor { get; set; } = 0.1;
    public bool CleanupPrevTrain { get; set; } = false;
    public int NInput { get; set; } = 2;
    public (int n, int nLayers) NHidden { get; set; } = (5, 3);
    public int NOutput { get; set; } = 1;
    public (double a, double b) Weight0 { get; set; } = (2, -1);
    public double PowerFactor { get; set; } = 1;
    public double FillFactor { get; set; } = 0.5;
    public double LinkFactor { get; set; } = 0.5;
    public double DampingCoeff { get; set; } = 0;

    public double Nu { get; set; } = 0.1;
    public double Alfa { get; set; } = 0.5;

    public (double[] input, double[] expected)[] Training { get; set; }
}
