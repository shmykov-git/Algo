using Aspose.ThreeD.Utilities;
using FluentAssertions;
using Model3D;
using Model3D.Extensions;
using NUnit.Framework;

namespace Model.Test
{
    public class VectorsTests
    {
        [Test]
        public void IsInsideTest()
        {
            var polygon = new[]
            {
                new Vector3(-1, -1, 0),
                new Vector3(0, -1.5, 0),
                new Vector3(1, -1, 0),
                new Vector3(1, 1, 0),
                new Vector3(0, 1.5, 0),
                new Vector3(-1, 1, 0),
            };

            var data = new (Vector3, bool)[]
            {
                (new Vector3(0, 0, 0), true),
                (new Vector3(-0.2, 0.3, 0), true),
                (new Vector3(-0.2, 0.3, 1000000), true),
                (new Vector3(-0.2, 0.3, -1000000), true),
                (new Vector3(-1.00001, 0, 0), false),
                (new Vector3(-1.00001, 0, 100000), false),
                (new Vector3(-1.00001, 0, -100000), false),
                (new Vector3(-0.8, 1.4, 0), false),
                (new Vector3(-0.8, 1.05, 0), true),
                (new Vector3(-0.8, 1.05, 100000), true),
            };

            foreach (var (x, expected) in data)
            {
                polygon.IsInside(x).Should().Be(expected);
            }
        }
    }
}