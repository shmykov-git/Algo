using FluentAssertions;
using Model.Libraries;
using Model3D;
using Model3D.Extensions;
using NUnit.Framework;

namespace Model.Test
{
    public class PlaneTests
    {
        [Test]
        public void ProjectionFnTest()
        {
            var a = new Vector3(1, 0, 0);
            var b = new Vector3(0, 1, 0);
            var c = new Vector3(0, 0, 0);

            var p = new Vector3(1, 2, 3);

            var plane = new Plane(a, b, c);

            var projection = plane.ProjectionFn(p);

            projection.Should().Be(new Vector3(1, 2, 0));
        }

        [Test]
        public void CubeIsInsideTest()
        {
            var cube = Shapes.Cube.Perfecto(2);

            var data = new (Vector3 vector, bool expected)[]
            {
                (new Vector3(0, 0, 0), true),
                (new Vector3(0.5001, 0, 0), true),
                (new Vector3(1.0001, 0, 0), false),
                (new Vector3(1.1, 1.1, 1.1), false),
            };

            foreach (var v in data)
            {
                cube.IsInside(v.vector).Should().Be(v.expected);
            }
        }

        [Test]
        public void IntersectionFnTest()
        {
            var plane = new Plane(new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(-1, -1, 0));
            var fn = plane.IntersectionFn;

            var data = new (Vector3 a, Vector3 b, Vector3 expected)[]
            {
                (new Vector3(1, 1, -3), new Vector3(1, 1, 1), new Vector3(1, 1, 0)),
                (new Vector3(1, 1, -3), new Vector3(-1, -1, 1), new Vector3(-0.5, -0.5, 0)),
            };

            foreach (var (a, b, expected) in data)
            {
               fn(a, b).Should().Be(expected); 
            }
        }
    }
}