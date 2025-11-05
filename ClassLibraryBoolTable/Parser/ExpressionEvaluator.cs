using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryBoolTable.Parser
{
    /// <summary>
    /// Вычислитель логической формулы в обратной польской записи (ОПЗ)
    /// </summary>
    public static class ExpressionEvaluator
    {
        /// <summary>
        /// Вычисляет значение формулы на конкретных значениях переменных.
        /// </summary>
        /// <param name="rpnTokens">Токены в ОПЗ</param>
        /// <param name="variableValues">Словарь: имя переменной -> bool значение</param>
        /// <returns>bool результат функции</returns>
        public static bool Evaluate(List<Token> rpnTokens, Dictionary<string, bool> variableValues)
        {
            var stack = new Stack<bool>();

            foreach (var token in rpnTokens)
            {
                switch (token.Type)
                {
                    case TokenType.Variable:
                        if (!variableValues.TryGetValue(token.Value, out bool val))
                            throw new Exception($"Переменная {token.Value} не задана");
                        stack.Push(val);
                        break;

                    case TokenType.Not:
                        if (stack.Count < 1) throw new Exception("Нехватка аргументов для !");
                        stack.Push(!stack.Pop());
                        break;

                    case TokenType.And:
                        if (stack.Count < 2) throw new Exception("Нехватка аргументов для &");
                        stack.Push(stack.Pop() & stack.Pop());
                        break;

                    case TokenType.Or:
                        if (stack.Count < 2) throw new Exception("Нехватка аргументов для |");
                        stack.Push(stack.Pop() | stack.Pop());
                        break;

                    case TokenType.Xor:
                        if (stack.Count < 2) throw new Exception("Нехватка аргументов для ^");
                        {
                            bool b2 = stack.Pop();
                            bool b1 = stack.Pop();
                            stack.Push(b1 ^ b2);
                        }
                        break;

                    case TokenType.Impl:
                        if (stack.Count < 2) throw new Exception("Нехватка аргументов для ->");
                        {
                            bool b2 = stack.Pop();
                            bool b1 = stack.Pop();
                            stack.Push(!b1 | b2); // импликация A -> B = !A | B
                        }
                        break;

                    case TokenType.Equiv:
                        if (stack.Count < 2) throw new Exception("Нехватка аргументов для =");
                        {
                            bool b2 = stack.Pop();
                            bool b1 = stack.Pop();
                            stack.Push(b1 == b2); // эквиваленция
                        }
                        break;

                    default:
                        throw new Exception($"Неизвестный токен {token.Value}");
                }
            }

            if (stack.Count != 1)
                throw new Exception("Ошибка вычисления: остаток элементов в стеке");

            return stack.Pop();
        }
    }
}
