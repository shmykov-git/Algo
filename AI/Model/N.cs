using AI.Libraries;
using AI.NBrain.Activators;

namespace AI.Model;

public class N
{
    public NActivator act;

    // <dynamic>
    public int i;           // order number 
    public int lv;          // level
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
    public double delta;    // training changing value
    public bool learned;    // pass throw ns while learn
    // </learn>

    public bool isInput => lv == 0;
    public bool isOutput => es.Count == 0; // todo: should be level depended (max lv)

    public bool IsLinked(N b) => es.Any(e => e.b == b);
    public E? GetLink(N b) => es.SingleOrDefault(e => e.b == b);

    public override string ToString() => $"{i}:{f:F2}";
}
