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
    }
}