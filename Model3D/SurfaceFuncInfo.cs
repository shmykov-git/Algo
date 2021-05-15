using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model3D.Libraries;
using System.Linq;

namespace Model
{
    public class SurfaceFuncInfo
    {
        public double UFrom;
        public double UTo;
        public int UN;
        public double VFrom;
        public double VTo;
        public int VN;
        public SurfaceFunc Fn;
        public bool UClosed;
        public bool VClosed;

        public Vector3[] GetPoints()
        {
            double un = (UClosed ? UN : (UN - 1));
            var ustep = (UTo - UFrom) / un;

            double vn = (VClosed ? VN : (VN - 1));
            var vstep = (VTo - VFrom) / vn;

            return (VN, UN).SelectRange((v, u) => Fn(UFrom + ustep * u, VFrom + vstep * v)).ToArray();
        }
    }
}
