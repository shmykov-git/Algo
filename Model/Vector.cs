using System;
using System.Linq;

namespace Model
{
    public class Vector
    {
        public int N;

        public double[] Values
        {
            get => v;
            set => v = value;
        }
        private double[] v;

        public double this[int j]
        {
            get => v[j];
            set => v[j] = value;
        }

        protected Vector(Vector vct)
        {
            v = vct.v;
            N = vct.N;
        }

        public Vector(params double[] a) : this(a.Length)
        {
            for (var j = 0; j < N; j++)
                v[j] = a[j];
        }

        public Vector(int N)
        {
            this.N = N;
            v = new double[N];
        }

        public static Vector operator *(Vector v1, double k)
        {
            return new Vector(Enumerable.Range(0, v1.N).Select(i => v1[i] * k).ToArray());
        }

        public static Vector operator /(Vector v1, double k)
        {
            return new Vector(Enumerable.Range(0, v1.N).Select(i => v1[i] / k).ToArray());
        }

        public static double operator *(Vector v1, Vector v2)
        {
            if (v1.N != v2.N)
                throw new InvalidOperationException("Vectors have different dimentions");

            return Enumerable.Range(0, v1.N).Select(i => v1[i] * v2[i]).Sum();
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            if (v1.N != v2.N)
                throw new InvalidOperationException("Vectors have different dimentions");

            return new Vector(Enumerable.Range(0, v1.N).Select(i => v1[i] + v2[i]).ToArray());
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            if (v1.N != v2.N)
                throw new InvalidOperationException("Vectors have different dimentions");

            return new Vector(Enumerable.Range(0, v1.N).Select(i => v1[i] - v2[i]).ToArray());
        }
    }
}