using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Aspose.ThreeD.Utilities;
using Meta.Extensions;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model.Tools;
using Model3D.Tools.Model;
using Vector2 = Model.Vector2;

namespace Model3D.Extensions
{
    public static class PolygonExtensions
    {        
        public static Shape MakeShape(this Polygon polygon, bool triangulate = false)
        {
            return polygon.Fill(triangulate).ToShape3();
        }

        public static Shape ToShape(this Polygon polygon, SolidOptions options)
        {
            var triangulate = options.TriangulationStrategy != TriangulationStrategy.None;
            var needVolume = options.ZVolume.HasValue;

            if (!needVolume && !triangulate)
                return new Shape
                {
                    Points2 = polygon.Points,
                    Convexes = new[] { polygon.Points.Index().ToArray() }
                };

            var trConvexes = options.TriangulationStrategy switch
            {
                TriangulationStrategy.Trio => FillEngine.Triangulate(polygon.Points, FillEngine.FindConvexes(polygon)),
                TriangulationStrategy.AngleSort => Triangulator.Triangulate(polygon, options.TriangulationFixFactor),
                TriangulationStrategy.SizeSort => Triangulator2.Triangulate(polygon, options.Copy<Triangulator2.Options>()),
                TriangulationStrategy.Ears => EarTriangulator.Triangulate(polygon, options.Copy<EarTriangulator.Options>()),
                _ => Array.Empty<int[]>()
            };

            var shape = new Shape()
            {
                Points2 = polygon.Points,
                Convexes = trConvexes
            }.Normalize();

            if (!needVolume)
                return shape;

            return shape.AddVolumeZ(options.ZVolume.Value, options.ZHardFaces);
        }

        public static Shape ToSingleConvexShape(this Polygon polygon) =>
            new Shape
            {
                Points2 = polygon.Points,
                Convexes = new[] { polygon.Points.Index().ToArray() }
            };

        public static Shape ToShape(this Polygon polygon, double? volume = null, bool triangulate = false)
        {
            var options = new SolidOptions()
            {
                ZVolume = volume
            };
            
            if (volume == null && !triangulate)
                options.TriangulationStrategy = TriangulationStrategy.None;

            return polygon.ToShape(options);
        }

        //public static Shape ToShape1(this Polygon polygon, double? volume = null, bool triangulate = false, double incorrectFix = 0, bool trioStrategy = false)
        //{
        //    if (!volume.HasValue && !triangulate)
        //        return new Shape
        //        {
        //            Points2 = polygon.Points,
        //            Convexes = new[] { polygon.Points.Index().ToArray() }
        //        };

        //    int[][] trConvexes;
        //    if (trioStrategy)
        //    {
        //        var convexes = FillEngine.FindConvexes(polygon);
        //        trConvexes = FillEngine.Triangulate(polygon.Points, convexes);
        //    }
        //    else
        //    {
        //        trConvexes = Triangulator.Triangulate(polygon, incorrectFix);
        //    }

        //    var shape0 = new Shape()
        //        {
        //            Points2 = polygon.Points,
        //            Convexes = trConvexes
        //        }.Normalize();

        //    if (!volume.HasValue)
        //        return shape0;

        //    return shape0.AddVolumeZ(volume.Value);
        //}

        public static Shape ToTriangulatedShape(this Polygon polygon, int countTriangle = 30, double? volume = null, double incorrectFix = 0)
        {
            var border = polygon.Border;
            var size = border.b - border.a;
            var center = 0.5 * (border.a + border.b);

            var n = countTriangle;
            var m = (int)(n * size.y / (size.x * 3d.Sqrt()));
            // что с масштабом?
            var net = Shapes.PlaneByTriangles(m, n).Mult(1.3 * size.x).Move(center.ToV3());
            var cutNet = net.Cut(polygon);
            var cutPoints = cutNet.Points2;

            var perimeters = cutNet.GetPerimeters();
            var perimeterPolygons = perimeters.Select(p => p.Select(i => cutPoints[i]).ToArray().ToPolygon()).ToArray();

            var borderPolygon = perimeterPolygons.Aggregate(polygon, JoinInside);
            var triangulatedBorder = borderPolygon.ToShape(triangulate: true);

            var triangulatedShape = (triangulatedBorder + cutNet).Normalize();

            if (volume == null)
                return triangulatedShape;

            var halfVolume = new Vector3(0, 0, volume.Value / 2);
            var nn = triangulatedShape.Points.Length;

            return new Shape
            {
                Points3 = new[]
                {
                    triangulatedShape.Points.Select(p => p.ToV3() - halfVolume),
                    triangulatedShape.Points.Select(p => p.ToV3() + halfVolume)
                }.ManyToArray(),

                Convexes = new[]
                {
                    triangulatedShape.Convexes.Select(c => c.Reverse().ToArray()),
                    triangulatedShape.Convexes.Transform(v => v + nn),
                    polygon.Points.Index().Reverse().SelectCirclePair((i, j) => new[] { i, i + nn, j + nn, j }).ToArray()
                }.ManyToArray()
            };
        }

        public static Polygon JoinInside(this Polygon polygon, Polygon joinPolygon)
        {
            var (i, j, _) = (polygon.Points.Length, joinPolygon.Points.Length)
                .SelectRange((i, j) => (i, j, len2: (polygon[i] - joinPolygon[j]).Len2))
                .OrderBy(v => v.len2).First();

            var points = new[]
            {
                polygon.Points[..(i+1)],
                joinPolygon.Points[..(j+1)].Reverse(),
                joinPolygon.Points[j..].Reverse(),
                polygon.Points[i..]
            }.SelectMany(v => v).ToArray();

            return new Polygon() {Points = points};
        }

        public static Polygon ComposeOthersToFirst(this Polygon[] polygons)
        {
            var internals = (polygons.Length).Range().Skip(1).Select(i => (0, i)).ToArray();

            return polygons.Compose(internals).First();
        }
       
        public static Polygon[] Compose(this Polygon[] polygons, (int main, int child)[] map, bool skipReverse = false)
        {
            // тут ошибка
            var getLevel = map.GetMapLevelFn();

            var excepts = map.Where(v=>getLevel(v.child).Even()).Select(v => v.child).ToHashSet();
            var includes = map.GroupBy(v => v.main).ToDictionary(gv => gv.Key, gv => gv.Select(v => v.child).ToArray());

            Polygon JoinPolygons(Polygon[] polygs)
            {
                if (polygs.Length == 1)
                    return polygs[0];

                var pairs = (polygs.Length, polygs.Length).UniquePairs()
                    .Select(p => (p.i, p.j, ln2: (polygs[p.i].Points, polygs[p.j].Points).SelectPair((x, y) => (y - x).Len2).Min()))
                    .OrderBy(v => v.ln2).ToArray();

                var i = pairs.First().i;
                var res = polygs[i];

                while (pairs.Length > 0)
                {
                    var pair = pairs.First(v => v.i == i || v.j == i);
                    pairs = pairs.Where(v => v.i != i && v.j != i).ToArray();
                    i = pair.i == i ? pair.j : pair.i;
                    res = res.Join(polygs[i]);
                }

                return res;
            }

            return polygons.Select((p, num) => (p, num))
                .Where(v => !excepts.Contains(v.num))
                .Select(v =>
                {
                    if (!includes.TryGetValue(v.num, out int[] takeList)) 
                        return v.p;

                    var innerPolygons = takeList.Select(i => skipReverse ? polygons[i] : polygons[i].Reverse());

                    return JoinPolygons(new []{ v.p }.Concat(innerPolygons).ToArray());
                })
                .ToArray();
        }

        public static Polygon[] ComposeObsolet(this Polygon[] polygons, (int takeI, int incJ)[] internals)
            => polygons.Compose(internals.Select(v => v.Reverse()).ToArray());

        
    }
}
