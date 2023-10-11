using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model3D.Libraries;
using System.Linq;

namespace Model
{
    public class SolidFuncInfo
    {
        public double UFrom;
        public double UTo;
        public int UN;
        public double VFrom;
        public double VTo;
        public int VN;
        public double LFrom;
        public double LTo;
        public int LN;
        public SolidFunc Fn;
        public bool UClosed;
        public bool VClosed;
        public bool LClosed;

        public Vector3[] GetPoints()
        {
            double un = (UClosed ? UN : (UN - 1));
            var ustep = (UTo - UFrom) / un;

            double vn = (VClosed ? VN : (VN - 1));
            var vstep = (VTo - VFrom) / vn;

            double ln = (LClosed ? LN : (LN - 1));
            var lstep = (LTo - LFrom) / ln;

            return (UN, VN, LN).SelectRange((u, v, l) => Fn(UFrom + ustep * u, VFrom + vstep * v, LFrom + lstep * l)).ToArray();
        }
    }
}
