using System;
using System.Linq;
using Model;
using Model.Extensions;
using Model3D.Extensions;

namespace Model3D.Libraries
{
    public static class Compounds
    {
        public static Shape SnakeSlots((int m, int n) slots, (double x, double y) size, Action stepFn, Func<Shape> getShapeFn)
        {
            var firstShape = getShapeFn();

            var shapes = slots.SelectSnakeRange((i, j) => (i, j)).Skip(1).Select(v =>
            {
                var (i, j) = v;

                stepFn();

                return getShapeFn().Move(j * (size.x + 1), -i * (size.y + 1), 0);
            });

            var shape = new[] { firstShape }.Concat(shapes).ToSingleShape();

            return shape;
        }
    }
}