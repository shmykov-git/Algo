namespace AI.Model;

public class NOptions
{
    public int Seed { get; set; } = 0;
    public double ShaffleFactor { get; set; } = 0.1;
    public bool CleanupTrainTails { get; set; } = false;
    public int NInput { get; set; } = 2;
    public int[] NHidden { get; set; } = [5];
    public int[] NHiddenUp { get; set; } = [5, 5];
    public int NOutput { get; set; } = 1;
    public (double a, double b) Weight0 { get; set; } = (2, -1);
    public double PowerFactor { get; set; } = 100;
    public double LinkFactor { get; set; } = 0.2;
    public double CrossLinkFactor { get; set; } = 0;
    public double GrowFactor { get; set; } = 0.5;
    public int GrowCount { get; set; } = 50;
    public double DampingCoeff { get; set; } = 0;

    public double Nu { get; set; } = 0.1;
    public double Alfa { get; set; } = 0.5;

    public (double[] input, double[] expected)[] Training { get; set; }
}
