using AI.Libraries;

namespace AI.Model;

public class N
{
    public double xx;     // signals sum
    public double x;     // signals result func
    public double delta; // training changing value

    //public NGroup g;    // group
    public E[] es = [];    // forward links

    public NFunc sigmoidFn;   // activator func
    public NFunc dampingFn;   // damping func

    public override string ToString() => $"{x:F2}";
}
