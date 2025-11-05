using ClassLibraryBoolTable.Util;
using NUnit.Framework;
using System.Linq;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace LogicLibrary.Tests
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void GenerateAllTuples_ShouldProduceCorrectCount()
        {
            var tuples = BitUtils.GenerateAllTuples(3);
            Assert.AreEqual(8, tuples.Count);
        }

        [TestMethod]
        public void DecodeFunction_ShouldProduceCorrectValues()
        {
            var values = BitUtils.DecodeFunction(3, 11); // 1011₂
            var expected = new bool[] { false, false, true, true, true, false, true, true };
            Assert.IsTrue(values.SequenceEqual(expected));
        }
    }
}
