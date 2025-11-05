using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryBoolTable.Parser
{
    /// <summary>
    /// Парсер, который преобразует токены в обратную польскую запись (ОПЗ).
    /// Используется алгоритм сортировочной станции Дейкстры.
    /// </summary>
    public static class ParserRpn
    {
        // Приоритеты операторов
        private static readonly Dictionary<TokenType, int> Precedence = new Dictionary<TokenType, int>
        {
            { TokenType.Not, 4 },
            { TokenType.And, 3 },
            { TokenType.Or, 2 },
            { TokenType.Xor, 2 },
            { TokenType.Impl, 1 },
            { TokenType.Equiv, 0 }
        };

        /// <summary>
        /// Преобразует список токенов в обратную польскую запись (ОПЗ)
        /// </summary>
        public static List<Token> ToRpn(List<Token> tokens)
        {
            var output = new List<Token>();
            var stack = new Stack<Token>();

            foreach (var token in tokens)
            {
                switch (token.Type)
                {
                    case TokenType.Variable:
                        output.Add(token);
                        break;

                    case TokenType.Not:
                    case TokenType.And:
                    case TokenType.Or:
                    case TokenType.Xor:
                    case TokenType.Impl:
                    case TokenType.Equiv:
                        while (stack.Count > 0 && stack.Peek().Type != TokenType.LParen &&
                               Precedence[stack.Peek().Type] >= Precedence[token.Type])
                        {
                            output.Add(stack.Pop());
                        }
                        stack.Push(token);
                        break;

                    case TokenType.LParen:
                        stack.Push(token);
                        break;

                    case TokenType.RParen:
                        while (stack.Count > 0 && stack.Peek().Type != TokenType.LParen)
                        {
                            output.Add(stack.Pop());
                        }
                        if (stack.Count == 0 || stack.Peek().Type != TokenType.LParen)
                            throw new Exception("Скобки не сбалансированы");
                        stack.Pop(); // убираем LParen
                        break;
                }
            }

            while (stack.Count > 0)
            {
                if (stack.Peek().Type == TokenType.LParen || stack.Peek().Type == TokenType.RParen)
                    throw new Exception("Скобки не сбалансированы");
                output.Add(stack.Pop());
            }

            return output;
        }
    }
}
