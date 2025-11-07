using ClassLibraryBoolTable.Service;
using ClassLibraryBoolTable.Parser;
using ClassLibraryBoolTable.Models;
using ClassLibraryBoolTable.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace LogicLibrary.Tests
{
    [TestClass]
    public class BooleanFunctionTests
    {
        [TestMethod]
        public void TruthTable_FromNumber_ShouldMatchExpected()
        {
            var table = new TruthTable(3);
            table.BuildFromNumber(11); // 11 = 1011
            var expectedResults = new bool[] { false, false, false, true, true, false, true, true };

            for (int i = 0; i < table.Table.Count; i++)
            {
                Assert.AreEqual(expectedResults[i], table.Table[i].Result);
            }
        }

        [TestMethod]
        public void DNF_ShouldBuildCorrectly()
        {
            var table = new TruthTable(2);
            table.BuildFromNumber(6); // 6 = 0110
            string dnf = DnfGenerator.BuildDNF(table.Table, table.VariableNames);

            Assert.AreEqual("(!x1 & x2) | (x1 & !x2)", dnf);
        }

        [TestMethod]
        public void KNF_ShouldBuildCorrectly()
        {
            var table = new TruthTable(2);
            table.BuildFromNumber(6); // 6 = 0110
            string knf = KnfGenerator.BuildKNF(table.Table, table.VariableNames);

            Assert.AreEqual("(x1 | x2) & (!x1 | !x2)", knf);
        }

        [TestMethod]
        public void FunctionComparer_ShouldDetectEquivalence()
        {
            var table1 = new TruthTable(2);
            table1.BuildFromNumber(6);

            var table2 = new TruthTable(2);
            table2.BuildFromFunction(vars => vars[0] ^ vars[1]);

            Assert.IsTrue(FunctionComparer.AreEquivalent(table1.Table, table2.Table, out var _));
        }
    }
}
