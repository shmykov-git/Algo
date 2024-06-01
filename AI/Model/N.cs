using AI.Libraries;

namespace AI.Model;


public class NGroup
{
    //public float threshold;
}

public class N
{
    public E[] es = [];
    public NGroup g;
    public float x;
    public float y;
    public NFunc activatorFn;
    public NFunc dampingFn;
}

public class E
{
    public float dw;
    public int dwCount;

    public float w0;
    public float w;
    public float f;

    public float fPrev;
    public float wPrev;
    public N n;
    public int[] influence;
}
