using Model.Extensions;
using Model.Libraries;

namespace AI.Model;

public class E
{
    // <dynamic>
    public N a;             // compute from
    public N b;             // compute to
    // </dynamic>

    // <learn>
    public double w;        // weight
    public double dw;       // back propagation learn value
    public bool unwanted;   // need to remove
    public double uW0;      // weight before unwanted
    // </learn>

    // <learn.avg>
    public double sumDw;    // for avg dw calculation
    // </learn.avg>

    public bool canBeRemoved => unwanted && w.Abs() < Values.Epsilon9;

    public override string ToString() => $"{w:F2}";
}
