namespace Model.Fourier
{
    public class Fr
    {
        public int n;
        public double dn;
        public double r;
        public double im;
        public double dis;
        public (double r, double i) c => (r, im);
        public double k => n + dn;

        public static Fr operator *(Fr fr, double a) => new Fr
        {
            n = fr.n,
            dn = fr.dn,
            r = fr.r * a,
            im = fr.im * a,
            dis = fr.dis
        };

        public static Fr operator /(Fr fr, double a) => new Fr
        {
            n = fr.n,
            dn = fr.dn,
            r = fr.r / a,
            im = fr.im / a,
            dis = fr.dis
        };

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

        public static implicit operator Fr((int n, double r, decimal dis) v) => new Fr()
        {
            n = v.n,
            r = v.r,
            dis = (double)v.dis
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

        public override string ToString() => im == 0 ? $"({n+dn}, {r}){(dis == 0 ? "" : $"[{dis}]")}" : $"({n + dn}, ({r}, {im})){(dis == 0 ? "" : $"[{dis}]")}";
    }
}