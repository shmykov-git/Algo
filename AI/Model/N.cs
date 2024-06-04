using AI.Libraries;

namespace AI.Model;

public class N
{
    // <static>
    public NFunc sigmoidFn; // activator func
    public NFunc dampingFn; // damping func
    // </static>

    // <dynamic>
    public int i;
    //public NGroup g;      // group
    public List<E> es = []; // forward links
    public E[] backEs = []; // back links
    // </dynamic>

    // <compute>
    public double f;        // signals result func (output)
    public double xx;       // signals sum (input)
    public bool computed;   // pass throw ns while compute
    // </compute>

    // <learn>
    public double avgF;
    public double delta;    // training changing value
    public bool learned;    // pass throw ns while learn
    // </learn>

    public bool isInput => backEs.Length == 0;
    public bool isOutput => es.Count == 0;

    public override string ToString() => $"{i}:{f:F2}";
}
