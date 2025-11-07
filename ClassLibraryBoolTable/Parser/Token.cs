using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryBoolTable.Parser
{
    /// <summary>
    /// Тип токена (лексемы) логической формулы.
    /// </summary>
    public enum TokenType
    {
        Variable,   // x1, x2
        Not,        // !, not
        And,        // &, and
        Or,         // |, or
        Xor,        // ^, xor
        Impl,       // -> импликация
        Equiv,      // = эквивалентность
        LParen,     // (
        RParen      // )
    }

    /// <summary>
    /// Токен логической формулы.
    /// </summary>
    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }

        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Type}: {Value}";
        }
    }
}
