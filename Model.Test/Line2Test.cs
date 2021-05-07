using Model.Extensions;
using NUnit.Framework;

namespace Model.Test
{
    public class Line2Test
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void FnTest()
        {
            Vector2 a = (1, 1);
            Vector2 b = (2, 3);

            Line2 l = (a, b);

            Assert.AreEqual(0, l.Fn(a));
            Assert.AreEqual(0, l.Fn(b));
        }

        [Test]
        public void IntersectTest()
        {
            Vector2 a = (1, 1);
            Vector2 b = (2, 3);
            Vector2 c = (0, 2);
            Vector2 d = (2, 1);
            Vector2 e = (0, -2);

            Line2 l1 = (a, b);
            Line2 l2 = (c, d);
            Line2 l3 = (e, d);

            Assert.AreEqual(true, l1.IsSectionIntersectedBy(l2));
            Assert.AreEqual(false, l1.IsSectionIntersectedBy(l3));
        }
    }
}