using System;
using System.Diagnostics;
using System.Linq;
using Model.Extensions;

namespace Model
{
    public class Matrix
    {
        public int M;
        public int N;
        private Vector[] m;

        public double this[int i, int j]
        {
            get => m[i][j];
            set => m[i][j] = value;
        }

        public Vector this[int i]
        {
            get => m[i];
            set => m[i] = value;
        }

        protected Matrix(Matrix mtx)
        {
            m = mtx.m;
            M = mtx.M;
            N = mtx.N;
        }

        public Matrix(double[][] aa) : this(aa.Length, aa[0].Length)
        {
            for (var i = 0; i < M; i++)
                for (var j = 0; j < N; j++)
                    m[i][j] = aa[i][j];
        }

        public Matrix(int M, int N)
        {
            this.M = M;
            this.N = N;
            m = M.SelectRange(_ => new Vector(N)).ToArray();
        }

        public Matrix(int N) : this(N, N) { }

        public double D()
        {
            if (M == 2 && N == 2)
                return m[0][0] * m[1][1] - m[0][1] * m[1][0];

            var d = 0.0;
            for (var j = 0; j < M; j++)
            {
                var sign = j % 2 == 0 ? 1 : -1;
                d += sign * m[0][j] * Minor(0, j).D();
            }

            return d;
        }

        //todo: Обратная матрица не тестировано
        public Matrix A_()
        {
            if (M != N)
                throw new ApplicationException("Can find back matrix only for cube matrix");

            var d = D();

            var m = new Matrix(N, N);
            for (var i = 0; i < N; i++)
                for (var j = 0; j < N; j++)
                {
                    var sign = (i + j) % 2 == 0 ? 1 : -1;
                    m[i, j] = sign * Minor(i, j).D() / d;
                }

            return m;
        }

        public Matrix Minor(int I, int J)
        {
            var mA = new Matrix(M - 1, N - 1);
            for (var i = 0; i < M - 1; i++)
                for (var j = 0; j < N - 1; j++)
                    mA[i][j] = m[i < I ? i : i + 1][j < J ? j : j + 1];
            return mA;
        }

        public Matrix InsCol(int J, Vector v)
        {
            var mIns = new Matrix(M, N);
            for (var i = 0; i < M; i++)
                for (var j = 0; j < N; j++)
                    mIns[i][j] = j == J ? v[i] : m[i][j];
            return mIns;
        }

        public Vector Kramer(Vector d)
        {
            var D0 = D();
            var Ds = Enumerable.Range(0, N).Select(i => InsCol(i, d).D());
            return new Vector(Ds.Select(Di => Di / D0).ToArray());
        }

        public (bool success, Vector result) KramerChecked(Vector d, double epsilon)
        {
            var D0 = D();
            if (D0.Abs() < epsilon)
                return (false, default);

            var Ds = Enumerable.Range(0, N).Select(i => InsCol(i, d).D());
            var res = new Vector(Ds.Select(Di => Di / D0).ToArray());

            return (true, res);
        }

        private void DebugM(string msg, Matrix m)
        {
            Debug.WriteLine(msg);
            for (var i = 0; i < m.M; i++)
            {
                var line = "";
                for (var j = 0; j < m.N; j++)
                {
                    line += $"{m[i][j]:N2}; ";
                }
                Debug.WriteLine($"({line})");
            }
        }


        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            if (m1.N != m2.M)
                throw new InvalidOperationException($"Matrix muliplication dimention error ({m1.M}, {m1.N}) x ({m2.M}, {m2.N})");

            var m = new Matrix(m1.M, m2.N);
            for (var i = 0; i < m1.M; i++)
                for (var j = 0; j < m2.N; j++)
                    m[i][j] = Enumerable.Range(0, m1.N).Sum(r => m1[i][r] * m2[r][j]);

            return m;
        }

        public static Matrix operator *(double a, Matrix m0)
        {
            var m = new Matrix(m0.M, m0.N);
            for (var i = 0; i < m0.M; i++)
                for (var j = 0; j < m0.N; j++)
                    m[i][j] = a * m0[i][j];

            return m;
        }

        public static Vector operator *(Matrix m, Vector v)
        {
            if (m.N != v.N)
                throw new InvalidOperationException($"Matrix & Vector muliplication dimention error ({m.M}, {m.N}) x ({v.N})");

            var vv = new Vector(m.M);
            for (var i = 0; i < m.M; i++)
                vv[i] = Enumerable.Range(0, m.M).Sum(j => m[i][j] * v[j]);

            return vv;
        }
    }
}
