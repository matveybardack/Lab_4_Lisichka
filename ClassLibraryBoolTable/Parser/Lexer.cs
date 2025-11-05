using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClassLibraryBoolTable.Parser
{
    /// <summary>
    /// Лексер: преобразует строку формулы в список токенов.
    /// </summary>
    public static class Lexer
    {
        // Regex для всех токенов
        private static readonly Regex TokenRegex = new Regex(
            @"(?<Var>x\d+)|(?<Not>!|not)|(?<And>&|and)|(?<Or>\||or)|(?<Xor>\^|xor)|(?<Impl>->)|(?<Eq>=)|(?<L>\()|(?<R>\))",
            RegexOptions.IgnoreCase);

        /// <summary>
        /// Преобразует строку формулы в список токенов.
        /// </summary>
        public static List<Token> Lex(string input)
        {
            var tokens = new List<Token>();
            string cleanInput = input.Replace(" ", ""); // убираем пробелы

            foreach (Match m in TokenRegex.Matches(cleanInput))
            {
                if (m.Groups["Var"].Success) tokens.Add(new Token(TokenType.Variable, m.Value));
                else if (m.Groups["Not"].Success) tokens.Add(new Token(TokenType.Not, m.Value));
                else if (m.Groups["And"].Success) tokens.Add(new Token(TokenType.And, m.Value));
                else if (m.Groups["Or"].Success) tokens.Add(new Token(TokenType.Or, m.Value));
                else if (m.Groups["Xor"].Success) tokens.Add(new Token(TokenType.Xor, m.Value));
                else if (m.Groups["Impl"].Success) tokens.Add(new Token(TokenType.Impl, m.Value));
                else if (m.Groups["Eq"].Success) tokens.Add(new Token(TokenType.Equiv, m.Value));
                else if (m.Groups["L"].Success) tokens.Add(new Token(TokenType.LParen, m.Value));
                else if (m.Groups["R"].Success) tokens.Add(new Token(TokenType.RParen, m.Value));
            }

            return tokens;
        }
    }
}
