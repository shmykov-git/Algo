using System.Collections.Generic;
using System.Linq;
using Model3D.AsposeModel;
using Model;
using Model.Extensions;

namespace Model3D.Extensions
{
    public static class ShapeCompoundExtensions
    {
        public static Shape CompoundOx(this IEnumerable<Shape> shapes, double distance = 0, Shape delimiterShape = null)
        {
            var shapesArr = shapes.ToArray();

            var dx = 0d;

            delimiterShape ??= Shape.Empty;
            var delLen = delimiterShape.SizeX;

            Shape GetItem(Shape s, bool isLast)
            {
                var sizeX = s.SizeX;
                var res = s.Move(dx, 0, 0);
                
                if (delimiterShape != null && !isLast)
                    res += delimiterShape.Move(dx+ sizeX + distance / 2, 0, 0);

                dx += sizeX + distance + delLen;

                return res;
            }

            return shapesArr.SelectWithIndex((s, ind) => GetItem(s, ind == shapesArr.Length - 1)).ToSingleShape();
        }

        public static Shape CompoundDirs(this IEnumerable<Shape> shapes, double distanceX = 0, double distanceY = 0, double distanceZ = 0)
        {
            var distance = new Vector3(distanceX, distanceY, distanceZ);

            var dirs = new[]
            {
                new Vector3(-1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, -1, 0),
                new Vector3(0, 0, 1),
                new Vector3(0, 0, -1),
            };

            var shifts = (dirs.Length).SelectRange(i => new Vector3(0, 0, 0)).ToArray();

            var i = -1;

            return shapes.Aggregate(Shape.Empty, (a, b) =>
            {
                i = (i + 1) % dirs.Length;
                
                if (b == null)
                    return a;

                shifts[i] += (b.Size + distance).MultC(dirs[i]);

                return a + b.Move(shifts[i]);
            });
        }
    }
}