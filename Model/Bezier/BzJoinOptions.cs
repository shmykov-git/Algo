using Model.Libraries;

namespace Model.Bezier;

public class BzJoinOptions
{
    public double Epsilon { get; set; } = Values.Epsilon9;
    public BzJoinType Type { get; set; } = BzJoinType.Line;
    public double Alfa { get; set; } = 0;
    public double Betta { get; set; } = 0;
    public double x { get; set; } = 1;
    public double y { get; set; } = 1;
    public double z { get; set; } = 1;

}
