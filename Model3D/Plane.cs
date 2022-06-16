using System;
using System.ComponentModel.DataAnnotations;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model3;
using Model3D.Extensions;

namespace Model3D
{
    public class Plane
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 ca => a - c;
        public Vector3 cb => b - c;
        public Vector3 Normal => ca.MultV(cb);
        public Vector3 NOne => Normal.Normalize();
        public Func<Vector3, double> Fn
        {
            get
            {
                var n = NOne;

                return x => n.MultS(x - c);
            }
        }

        public Func<Vector3, Vector3> ProjectionFn
        {
            get
            {
                var n = NOne;

                return x => x - n * n.MultS(x - c);
            }
        }

        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }
}
