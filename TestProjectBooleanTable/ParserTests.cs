using ClassLibraryBoolTable.Parser;
using NUnit.Framework;
using System.Collections.Generic;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace LogicLibrary.Tests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void Lexer_ShouldTokenizeFormula()
        {
            var tokens = Lexer.Lex("(x1 | x2) -> !x3");
            Assert.AreEqual(8, tokens.Count); // (, x1, |, x2, ), ->, !, x3
        }

        [TestMethod]
        public void ParserRpn_ShouldProduceCorrectRpn()
        {
            var tokens = Lexer.Lex("x1 & x2");
            var rpn = ParserRpn.ToRpn(tokens);

            // Стековая проверка: "!x1 x2 &"
            Assert.AreEqual(TokenType.Variable, rpn[1].Type); // x2
            Assert.AreEqual(TokenType.And, rpn[2].Type);      // &
        }

        [TestMethod]
        public void ExpressionEvaluator_ShouldComputeCorrectly()
        {
            var tokens = Lexer.Lex("x1 -> x2");
            var rpn = ParserRpn.ToRpn(tokens);
            var values = new Dictionary<string, bool> { { "x1", true }, { "x2", false } };

            bool result = ExpressionEvaluator.Evaluate(rpn, values);
            Assert.IsFalse(result); // true -> false = false
        }
    }
}
