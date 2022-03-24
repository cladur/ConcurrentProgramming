using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;

namespace NumberConverterTest
{
    [TestClass]
    public class BinDecTest
    {
        [TestMethod]
        public void DecToBinTest()
        {
            CollectionAssert.AreEqual(new List<int> { 1, 1, 0, 1 }, NumberConverter.BinDec.DecToBin(13));
        }

        [TestMethod]
        public void BinToDecTest()
        {
            Assert.AreEqual(19, NumberConverter.BinDec.BinToDec(new List<int> { 1, 0, 0, 1, 1 }));
        }
    }
}