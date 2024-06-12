namespace AI.Model;

public class NOptions
{
    public int Seed { get; set; } = 0;
    public double ShaffleFactor { get; set; } = 0.1;
    public bool CleanupTrainTails { get; set; } = false;
    public (int i, int j, double w)[][] Model { get; set; } = [];
    public (int i, int j)[][] Graph { get; set; } = [];
    public (int i, int j)[][] UpGraph { get; set; } = [];
    public int[] Topology { get; set; } = [2, 5, 1];
    public int[] UpTopology { get; set; } = [2, 5, 5, 1];
    public bool AllowGrowing { get; set; } = false;
    public (double a, double b) Weight0 { get; set; } = (2, -1);
    public (double a, double b)? PowerWeight0 { get; set; } = null;
    public double PowerFactor { get; set; } = 100;
    public double SymmetryFactor { get; set; } = 0;
    public double LinkFactor { get; set; } = 0.2;
    public double CrossLinkFactor { get; set; } = 0;
    public double DampingCoeff { get; set; } = 0;

    public double Nu { get; set; } = 0.1;
    public double Alfa { get; set; } = 0.5;

    public (double[] input, double[] expected)[] Training { get; set; }
}
