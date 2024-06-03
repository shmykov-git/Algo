using AI.Libraries;

namespace AI.Model;

public class N
{
    public float xx;     // signals sum
    public float x;     // signals result func
    public float delta; // training changing value

    //public NGroup g;    // group
    public E[] es = [];    // forward links

    public NFunc sigmoidFn;   // activator func
    public NFunc dampingFn;   // damping func

    public override string ToString() => $"{x:F2}";
}
