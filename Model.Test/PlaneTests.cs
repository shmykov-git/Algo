using Aspose.ThreeD.Utilities;
using FluentAssertions;
using Model3D;
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
    }
}