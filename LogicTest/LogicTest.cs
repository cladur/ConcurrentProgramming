using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Logic;

namespace LogicTest
{
    [TestClass]
    public class LogicTest
    {
        [TestMethod]
        public void MockTest()
        {
            var dataStorage = new Mock<Data.DataStorageAbstract>();
            var logic = LogicAbstract.CreateInstance(dataStorage.Object, 5, 5);
        }

        [TestMethod]
        public void AddNBallsTest()
        {
            float boxWidth = 1000.0f;
            float boxHeight = 1000.0f;
            LogicAbstract logic = LogicAbstract.CreateInstance(boxWidth, boxHeight);
            logic.AddNBalls(10);
            for (int i = 0; i < logic.GetObservableCollection().Count; i++)
            {
                Assert.IsTrue(logic.GetObservableCollection()[i].IsInside(new Data.Vector2(boxWidth, boxHeight)));
            }
        }
    }
}