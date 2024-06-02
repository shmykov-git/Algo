namespace AI.Model;

public class NOptions
{
    public int Seed { get; set; } = 0;
    public int NInput { get; set; } = 2;
    public (int n, int nLayers) NHidden { get; set; } = (5, 3);
    public int NOutput { get; set; } = 1;
    public (float a, float b) BaseWeightFactor { get; set; } = (0.00008f, 0.00001f);
    public float FillFactor { get; set; } = 0.5f;
    public float LinkFactor { get; set; } = 0.5f;
    public float DampingCoeff { get; set; } = -0.0001f;

    public float Nu { get; set; } = 0.1f;
    public float Alfa { get; set; } = 0.5f;

    public (float[] input, float[] expected)[] Training { get; set; }
}
