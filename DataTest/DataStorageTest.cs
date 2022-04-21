using Microsoft.VisualStudio.TestTools.UnitTesting;
using Data;

namespace DataTest
{
    [TestClass]
    public class DataStorageTest
    {
        [TestMethod]
        public void AddGetRemoveTest()
        {
            DataStorageAbstract dataStorage = DataStorageAbstract.CreateInstance();
            Vector2 pos = new Vector2(1, 1);
            Vector2 vec = new Vector2(4, 4);
            Ball ball = new Ball(pos, vec, 5);
            dataStorage.Add(ball);
            Assert.AreEqual(1, dataStorage.Count());
            Assert.AreEqual(ball, dataStorage.Get(0));
            dataStorage.Remove(ball);
            Assert.AreEqual(0, dataStorage.Count());
            dataStorage.Add(ball);
            dataStorage.RemoveAt(0);
            Assert.AreEqual(0, dataStorage.Count());
        }
    }
}