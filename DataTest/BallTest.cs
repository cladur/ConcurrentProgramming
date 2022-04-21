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
            Ball ball = new Ball(pos, vec, 5);
            ball.Move(0.5f);
            Vector2 expected_pos = new Vector2(3, 3);
            Assert.AreEqual(expected_pos.X, ball.Position.X);
            Assert.AreEqual(expected_pos.Y, ball.Position.Y);
        }
    }
}