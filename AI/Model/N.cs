using AI.Libraries;

namespace AI.Model;

public class N
{
    public int i;
    public double xx;       // signals sum (input)
    public double f;        // signals result func (output)
    public double delta;    // training changing value

    //public NGroup g;    // group
    public List<E> es = [];    // forward links
    public E[] backEs = [];    // back links

    public NFunc sigmoidFn;   // activator func
    public NFunc dampingFn;   // damping func

    public bool learned;

    public override string ToString() => $"{i}:{f:F2}";
}
