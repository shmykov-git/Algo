namespace AI.Images;

public class NImageOptions
{
    public double BoxM { get; set; } = 0.75;
    public int MaxValue { get; set; } = 1;
    public bool HasSign { get; set; } = false;

    public NImageBorderType BorderType { get; set; } = NImageBorderType.Padding;   
}
