using Model.Extensions;
using Model3D.Libraries;
using System;
using System.Drawing;
using System.Linq;
using Model3D;

namespace Model
{
    public class SurfaceFuncInfo
    {
        public SurfaceFunc Fn;
        public ConvexTransformFunc? ConvexTransformFn;

        public double UFrom;
        public double UTo;
        public int UN;
        public double VFrom;
        public double VTo;
        public int VN;
        public bool UClosed;
        public bool VClosed;

        public Vector3[] GetPoints()
        {
            double un = (UClosed ? UN : (UN - 1));
            var ustep = (UTo - UFrom) / un;

            double vn = (VClosed ? VN : (VN - 1));
            var vstep = (VTo - VFrom) / vn;
            
            if (ConvexTransformFn == null)
                return (VN, UN).SelectRange((v, u) => Fn(UFrom + ustep * u, VFrom + vstep * v)).ToArray();
            else
                return (VN, UN).SelectRange((v, u) => ConvexTransformFn(u, v, Fn(UFrom + ustep * u, VFrom + vstep * v))).ToArray();
        }
    }
}
