using Model;
using Model.Extensions;
using System.Linq;

namespace Model3D.Extensions
{
    public static class MaterialExtensions
    {
        public static Material[] ThisOrDefault(this Material[] materials) => materials ?? materials.Index().Select(i => (Material)null).ToArray();
    }
}
