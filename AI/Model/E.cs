namespace AI.Model;

public class E
{
    // <dynamic>
    public N a;
    public N b;
    // </dynamic>

    // <learn>
    public double w;
    // </learn>

    // <learn.avg>
    public double dw;
    public double sumDw;
    // </learn.avg>

    public override string ToString() => $"{w:F2}";
}
