using Microsoft.VisualStudio.TestTools.UnitTesting;
using Data;

namespace DataTest
{
    [TestClass]
    public class BallTest
    {
        [TestMethod]
        public void MoveTest()
        {
            Vector2 pos = new Vector2(1, 1);
            Vector2 vec = new Vector2(4, 4);
            Ball ball = new Ball(pos, vec, 5, 5);
            ball.Move(0.5f);
            Vector2 expected_pos = new Vector2(3, 3);
            Assert.AreEqual(expected_pos.X, ball.Position.X);
            Assert.AreEqual(expected_pos.Y, ball.Position.Y);
        }

        [TestMethod]
        public void IsIntersectingTest()
        {
            Vector2 pos = new Vector2(1, 1);
            Vector2 vec = new Vector2(4, 4);
            Ball ball = new Ball(pos, vec, 5, 5);
            Vector2 pos2 = new Vector2(1, 1);
            Ball ball2 = new Ball(pos2, vec, 5, 5);
            Vector2 pos3 = new Vector2(50, 1);
            Ball ball3 = new Ball(pos3, vec, 5, 5);
            Assert.IsTrue(ball.isIntersecting(ball2));
            Assert.IsFalse(ball.isIntersecting(ball3));
        }

        [TestMethod]
        public void IsInsideTest()
        {
            Vector2 pos = new Vector2(25, 25);
            Vector2 vec = new Vector2(4, 4);
            Ball ball = new Ball(pos, vec, 5, 5);
            Vector2 pos2 = new Vector2(100, 1);
            Ball ball2 = new Ball(pos2, vec, 5, 5);

            Vector2 boxSize = new Vector2(50, 50);
            Assert.IsTrue(ball.IsInside(boxSize));
            Assert.IsFalse(ball2.IsInside(boxSize));
        }
    }
}