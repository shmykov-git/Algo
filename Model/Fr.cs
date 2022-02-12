namespace Model
{
    public class Fr
    {
        public int n;
        public double dn;
        public double r;
        public double im;

        public static implicit operator Fr((int n, double r) v) => new Fr()
        {
            n = v.n,
            r = v.r
        };

        public static implicit operator Fr((double n, double r) v) => new Fr()
        {
            n = (int)v.n,
            dn = v.n - (int)v.n,
            r = v.r
        };

        public static implicit operator Fr((int n, double r, double dn) v) => new Fr()
        {
            n = v.n,
            dn = v.dn,
            r = v.r
        };

        public static implicit operator Fr((int n, (double r, double im) rm) v) => new Fr()
        {
            n = v.n,
            r = v.rm.r,
            im = v.rm.im
        };

        public static implicit operator Fr((int n, (double r, double im) rm, double dn) v) => new Fr()
        {
            n = v.n,
            dn = v.dn,
            r = v.rm.r,
            im = v.rm.im
        };
    }
}