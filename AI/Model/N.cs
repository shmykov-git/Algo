using AI.Libraries;
using AI.NBrain;
using AI.NBrain.Activators;

namespace AI.Model;

public class N
{
    // <static>
    public NActivator act;
    public NModel model;
    // <static>

    // <dynamic>
    public int i;           // order number 
    public int lv;          // level
    public int ii;          // level order number
    public List<E> es = []; // forward links
    public E[] backEs = []; // back links
    // </dynamic>

    // <compute>
    public double f;        // output computed result
    public double ff;       // output precomputed result
    public double xx;       // input sum of computed signals
    public bool computed;   // flag to compute topology graph
    public bool preComputed;// flag to compute topology graph level
    // </compute>

    // <learn>
    public bool believed;               // no learn just believe
    public Func<N, double> believedFn;  // believe to this truth
    public double delta;    // training changing value
    public bool learned;    // pass throw ns while learn
    // </learn>

    public bool isInput => lv == 0;
    public bool isOutput => model.maxLv == lv;

    public bool IsLinked(N b) => es.Any(e => e.b == b);
    public E? GetLink(N b) => es.SingleOrDefault(e => e.b == b);

    public override string ToString() => $"{i}:{f:F2}";
}
