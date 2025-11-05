using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

// Пока нейронка, сейчас переделаю на нормальный класс
namespace LogicFunctionsLib
{
    #region Tokens / Lexer / RPN

    public enum TokenType { Variable, Const, Op, LParen, RParen }
    public record Token(TokenType Type, string Text);

    public static class Lexer
    {
        // Поддерживаемые лексемы: идентификаторы (x1, x2...), числа 0/1, скобки, операторы:
        // ! not, ~ (NOT alias)
        // & and, &&, and
        // | or, ||, or
        // ^ xor, xor
        // -> => implies
        // = == <=> equiv (we accept = and == and <-> and <=>)
        // пробелы игнорируем
        private static readonly Regex tokenRegex = new(@"\s*(=>|->|<->|<=>|==|!=|&&|\|\||[!~]|[&\|^()=<>]|[A-Za-z_]\w*|0|1)\s*",
                                                        RegexOptions.Compiled);

        public static IReadOnlyList<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();
            var m = tokenRegex.Matches(input);
            var pos = 0;
            foreach (Match match in m)
            {
                if (match.Index != pos)
                    throw new ArgumentException($"Unexpected character at position {pos} near '{input.Substring(pos)}'.");

                var s = match.Value.Trim();
                pos += match.Length;

                if (s == "(") tokens.Add(new Token(TokenType.LParen, s));
                else if (s == ")") tokens.Add(new Token(TokenType.RParen, s));
                else if (s == "!" || s == "~" || s.Equals("not", StringComparison.OrdinalIgnoreCase))
                    tokens.Add(new Token(TokenType.Op, "NOT"));
                else if (s == "&" || s == "&&" || s.Equals("and", StringComparison.OrdinalIgnoreCase))
                    tokens.Add(new Token(TokenType.Op, "AND"));
                else if (s == "|" || s == "||" || s.Equals("or", StringComparison.OrdinalIgnoreCase))
                    tokens.Add(new Token(TokenType.Op, "OR"));
                else if (s == "^" || s.Equals("xor", StringComparison.OrdinalIgnoreCase))
                    tokens.Add(new Token(TokenType.Op, "XOR"));
                else if (s == "->" || s == "=>" || s == "implies")
                    tokens.Add(new Token(TokenType.Op, "IMPL"));
                else if (s == "=" || s == "==" || s == "<->" || s == "<=>" || s.Equals("equiv", StringComparison.OrdinalIgnoreCase))
                    tokens.Add(new Token(TokenType.Op, "EQU"));
                else if (s == "0" || s == "1")
                    tokens.Add(new Token(TokenType.Const, s));
                else
                    tokens.Add(new Token(TokenType.Variable, s));
            }

            if (pos != input.Length)
                throw new ArgumentException($"Unexpected trailing characters at position {pos}.");

            return tokens;
        }
    }

    public static class RpnConverter
    {
        // Приоритеты: NOT(4) > AND(3) > XOR(2) > OR(1) > IMPL(0) > EQU(0)
        private static readonly Dictionary<string, int> prec = new()
        {
            ["NOT"] = 5,
            ["AND"] = 4,
            ["XOR"] = 3,
            ["OR"] = 2,
            ["IMPL"] = 1,
            ["EQU"] = 0
        };

        private static bool IsRightAssociative(string op) => op == "NOT" || op == "IMPL" || op == "EQU";

        public static IReadOnlyList<Token> ToRpn(IReadOnlyList<Token> tokens)
        {
            var output = new List<Token>();
            var ops = new Stack<Token>();

            foreach (var t in tokens)
            {
                if (t.Type == TokenType.Variable || t.Type == TokenType.Const)
                    output.Add(t);
                else if (t.Type == TokenType.Op)
                {
                    while (ops.Count > 0 && ops.Peek().Type == TokenType.Op)
                    {
                        var top = ops.Peek();
                        int pTop = prec[top.Text];
                        int pCur = prec[t.Text];
                        if ((IsRightAssociative(t.Text) && pCur < pTop) ||
                            (!IsRightAssociative(t.Text) && pCur <= pTop))
                        {
                            output.Add(ops.Pop());
                        }
                        else break;
                    }
                    ops.Push(t);
                }
                else if (t.Type == TokenType.LParen)
                {
                    ops.Push(t);
                }
                else if (t.Type == TokenType.RParen)
                {
                    bool found = false;
                    while (ops.Count > 0)
                    {
                        var x = ops.Pop();
                        if (x.Type == TokenType.LParen) { found = true; break; }
                        output.Add(x);
                    }
                    if (!found) throw new ArgumentException("Mismatched parentheses.");
                }
            }

            while (ops.Count > 0)
            {
                var t = ops.Pop();
                if (t.Type == TokenType.LParen || t.Type == TokenType.RParen)
                    throw new ArgumentException("Mismatched parentheses.");
                output.Add(t);
            }

            return output;
        }
    }

    #endregion

    #region Evaluator / Formula

    public static class Evaluator
    {
        // Evaluate RPN tokens for a given variable assignment (mapping x1->bool, x2->bool ...)
        // Variables are strings like x1, x2, a, b, etc. We'll expect UI to use x1..xn
        public static bool EvalRpn(IReadOnlyList<Token> rpn, IReadOnlyDictionary<string, bool> assignment)
        {
            var st = new Stack<bool>();

            foreach (var t in rpn)
            {
                if (t.Type == TokenType.Const)
                {
                    st.Push(t.Text == "1");
                    continue;
                }
                if (t.Type == TokenType.Variable)
                {
                    if (!assignment.TryGetValue(t.Text, out var val))
                        throw new KeyNotFoundException($"Variable '{t.Text}' not in assignment.");
                    st.Push(val);
                    continue;
                }
                // operator
                switch (t.Text)
                {
                    case "NOT":
                        {
                            if (st.Count < 1) throw new ArgumentException("NOT requires 1 operand.");
                            var a = st.Pop();
                            st.Push(!a);
                            break;
                        }
                    case "AND":
                        {
                            if (st.Count < 2) throw new ArgumentException("AND requires 2 operands.");
                            var b = st.Pop(); var a = st.Pop();
                            st.Push(a & b);
                            break;
                        }
                    case "OR":
                        {
                            if (st.Count < 2) throw new ArgumentException("OR requires 2 operands.");
                            var b = st.Pop(); var a = st.Pop();
                            st.Push(a | b);
                            break;
                        }
                    case "XOR":
                        {
                            if (st.Count < 2) throw new ArgumentException("XOR requires 2 operands.");
                            var b = st.Pop(); var a = st.Pop();
                            st.Push(a ^ b);
                            break;
                        }
                    case "IMPL":
                        {
                            if (st.Count < 2) throw new ArgumentException("IMPL requires 2 operands.");
                            var b = st.Pop(); var a = st.Pop();
                            st.Push((!a) | b); // a->b == !a or b
                            break;
                        }
                    case "EQU":
                        {
                            if (st.Count < 2) throw new ArgumentException("EQU requires 2 operands.");
                            var b = st.Pop(); var a = st.Pop();
                            st.Push(a == b);
                            break;
                        }
                    default:
                        throw new NotSupportedException($"Operator {t.Text} is not supported in evaluation.");
                }
            }

            if (st.Count != 1) throw new ArgumentException("Bad RPN expression.");
            return st.Pop();
        }
    }

    public class ParsedFormula
    {
        public string Original { get; }
        public IReadOnlyList<Token> Tokens { get; }
        public IReadOnlyList<Token> Rpn { get; }
        public IReadOnlyCollection<string> Variables { get; }

        public ParsedFormula(string original, IReadOnlyList<Token> tokens, IReadOnlyList<Token> rpn)
        {
            Original = original;
            Tokens = tokens;
            Rpn = rpn;
            Variables = tokens.Where(t => t.Type == TokenType.Variable)
                              .Select(t => t.Text)
                              .Distinct()
                              .OrderBy(name => name, StringComparer.Ordinal)
                              .ToList()
                              .AsReadOnly();
        }

        public bool Evaluate(IDictionary<string, bool> assignment)
            => Evaluator.EvalRpn(Rpn, new Dictionary<string, bool>(assignment, StringComparer.OrdinalIgnoreCase));
    }

    public static class FormulaParser
    {
        public static ParsedFormula Parse(string formula)
        {
            var tokens = Lexer.Tokenize(formula);
            var rpn = RpnConverter.ToRpn(tokens);
            return new ParsedFormula(formula, tokens, rpn);
        }
    }

    #endregion

    #region Truth table / DNF / KNF / Utils

    public record TruthRow(bool[] Vars, bool Value);

    public class TruthTable
    {
        public string[] VarNames { get; }
        public TruthRow[] Rows { get; }

        public TruthTable(string[] varNames, TruthRow[] rows)
        {
            VarNames = varNames;
            Rows = rows;
        }

        // DNF in base {¬, ∧, ∨} (non-simplified): disjunction of minterms where value == true
        public string GetDNF()
        {
            var n = VarNames.Length;
            var terms = new List<string>();
            foreach (var r in Rows.Where(r => r.Value))
            {
                var lits = new List<string>();
                for (int i = 0; i < n; i++)
                {
                    lits.Add(r.Vars[i] ? VarNames[i] : $"!{VarNames[i]}");
                }
                terms.Add($"({string.Join(" & ", lits)})");
            }
            if (terms.Count == 0) return "0";
            return string.Join(" | ", terms);
        }

        // KNF: conjunction of clauses for rows where value == false
        public string GetKNF()
        {
            var n = VarNames.Length;
            var clauses = new List<string>();
            foreach (var r in Rows.Where(r => !r.Value))
            {
                var lits = new List<string>();
                for (int i = 0; i < n; i++)
                {
                    // note: clause is disjunction, so use variable if it was 0 in row becomes variable (not negated)
                    // For KNF we use (x1 | !x2 | x3) where row had x1=0 -> x1, x2=1 -> !x2
                    lits.Add(r.Vars[i] ? $"!{VarNames[i]}" : VarNames[i]);
                }
                clauses.Add($"({string.Join(" | ", lits)})");
            }
            if (clauses.Count == 0) return "1";
            return string.Join(" & ", clauses);
        }

        // Cost metrics for the *raw* DNF/KNF (non-simplified)
        public (int literalCount, int conjCount, int disjCount) GetDNFCost()
        {
            var n = VarNames.Length;
            var minterms = Rows.Count(r => r.Value);
            int literals = minterms * n;
            int conj = minterms;
            int disj = 1; // whole DNF is disjunction of minterms
            if (minterms == 0) { literals = 0; conj = 0; disj = 0; }
            return (literals, conj, disj);
        }

        public (int literalCount, int conjCount, int disjCount) GetKNFCost()
        {
            var n = VarNames.Length;
            var clauses = Rows.Count(r => !r.Value);
            int literals = clauses * n;
            int conj = 1; // whole KNF is conjunction of clauses
            int disj = clauses;
            if (clauses == 0) { literals = 0; conj = 0; disj = 0; }
            return (literals, conj, disj);
        }
    }

    public static class TruthTableGenerator
    {
        // ВАЖНО: Конвенция (явно документируемая):
        // Перечисляем все кортежи x1..xn в порядке возрастания двоичного числа,
        // где x1 — старший бит, xn — младший бит.
        // Для индекса k in [0..2^n-1], бинарное представление k (n бит) соответствует (x1..xn).
        // Значение f для кортежа с индексом k получается как (num >> k) & 1
        //
        // Пример: n=3, индексы и кортежи:
        // k=0 -> 000 (x1=0,x2=0,x3=0)
        // k=1 -> 001
        // ...
        // k=7 -> 111
        //
        // Для num=11 (decimal) -> 11 decimal = 00001011 (8-bit). Тогда
        // k=0 => bit0 = (11>>0)&1 = 1 => f(000)=1
        // k=1 => bit1 = (11>>1)&1 = 1 => f(001)=1
        // k=2 => 0 => f(010)=0
        // k=3 => 1 => f(011)=1
        // k=4..7 => 0
        //
        // Поэтому таблица для n=3,num=11: rows for 000..111 -> values [1,1,0,1,0,0,0,0]
        //
        public static TruthTable FromNumber(int n, ulong num, string varPrefix = "x")
        {
            if (n <= 0 || n > 62) throw new ArgumentOutOfRangeException(nameof(n));
            int rows = 1 << n;
            var names = Enumerable.Range(1, n).Select(i => varPrefix + i).ToArray();
            var data = new TruthRow[rows];
            for (int k = 0; k < rows; k++)
            {
                var vals = new bool[n];
                int mask = 1 << (n - 1);
                for (int i = 0; i < n; i++)
                {
                    // x1 is MSB of k: compute bit i of k where i=0 -> x1 corresponds to bit (n-1)
                    int bit = (k >> (n - 1 - i)) & 1;
                    vals[i] = bit == 1;
                }
                // According to convention, value is (num >> k) & 1 (LSB at k=0)
                bool value = ((num >> k) & 1UL) == 1UL;
                data[k] = new TruthRow(vals, value);
            }
            return new TruthTable(names, data);
        }

        public static TruthTable FromFormula(ParsedFormula parsed)
        {
            var vars = parsed.Variables.ToArray();
            int n = vars.Length;
            if (n == 0)
            {
                // constant formula: evaluate without variables; but implement by one-row table
                var rowsi = new[] { new TruthRow(new bool[0], parsed.Evaluate(new Dictionary<string, bool>())) };
                return new TruthTable(Array.Empty<string>(), rowsi);
            }
            int rowsCount = 1 << n;
            var rows = new TruthRow[rowsCount];
            for (int k = 0; k < rowsCount; k++)
            {
                var assignment = new Dictionary<string, bool>(StringComparer.Ordinal);
                for (int i = 0; i < n; i++)
                {
                    // x1 is MSB: take bit (n-1-i) of k
                    bool bit = ((k >> (n - 1 - i)) & 1) == 1;
                    assignment[vars[i]] = bit;
                }
                var val = parsed.Evaluate(assignment);
                rows[k] = new TruthRow(vars.Select(v => assignment[v]).ToArray(), val);
            }
            return new TruthTable(vars, rows);
        }
    }

    #endregion

    #region UI API / Presets / Equivalence

    public record TableResult(TruthTable Table, string DNF, string KNF,
                              (int lit, int conj, int disj) DNFcost,
                              (int lit, int conj, int disj) KNFcost);

    public static class LogicService
    {
        // Generate from number
        public static TableResult GenerateFromNumber(int n, ulong num, string varPrefix = "x")
        {
            var tt = TruthTableGenerator.FromNumber(n, num, varPrefix);
            var dnf = tt.GetDNF();
            var knf = tt.GetKNF();
            return new TableResult(tt, dnf, knf, tt.GetDNFCost(), tt.GetKNFCost());
        }

        // Parse formula and evaluate table
        public static TableResult GenerateFromFormula(string formula)
        {
            var parsed = FormulaParser.Parse(formula);
            var tt = TruthTableGenerator.FromFormula(parsed);
            var dnf = tt.GetDNF();
            var knf = tt.GetKNF();
            return new TableResult(tt, dnf, knf, tt.GetDNFCost(), tt.GetKNFCost());
        }

        // Equivalence: compare two sources (either number or formula). Returns (areEqual, firstCounterexampleRowIndex, row)
        // For numbers, supply (isNumber=true, n, num, formula=null)
        public static (bool equal, int? counterexampleIndex, TruthRow? rowA, TruthRow? rowB) CompareNumberToFormula(int n, ulong num, string formula)
        {
            var tA = TruthTableGenerator.FromNumber(n, num);
            var parsed = FormulaParser.Parse(formula);
            var tB = TruthTableGenerator.FromFormula(parsed);

            EnsureSameVarCountOrNormalize(ref tA, ref tB);

            for (int i = 0; i < tA.Rows.Length; i++)
            {
                if (tA.Rows[i].Value != tB.Rows[i].Value)
                    return (false, i, tA.Rows[i], tB.Rows[i]);
            }
            return (true, null, null, null);
        }

        public static (bool equal, int? counterexampleIndex, TruthRow? rowA, TruthRow? rowB) CompareFormulas(string f1, string f2)
        {
            var p1 = FormulaParser.Parse(f1);
            var p2 = FormulaParser.Parse(f2);
            var t1 = TruthTableGenerator.FromFormula(p1);
            var t2 = TruthTableGenerator.FromFormula(p2);

            EnsureSameVarCountOrNormalize(ref t1, ref t2);

            for (int i = 0; i < t1.Rows.Length; i++)
            {
                if (t1.Rows[i].Value != t2.Rows[i].Value)
                    return (false, i, t1.Rows[i], t2.Rows[i]);
            }
            return (true, null, null, null);
        }

        // Try to normalize variable names if different: if counts differ, we cannot compare. If same count but names differ,
        // we assume order x1..xn on both and re-generate from formulas to ensure same var order (TruthTableGenerator does var order from parsed.Variables).
        private static void EnsureSameVarCountOrNormalize(ref TruthTable a, ref TruthTable b)
        {
            if (a.VarNames.Length != b.VarNames.Length)
                throw new InvalidOperationException("Cannot compare functions with different number of variables.");

            // If var names differ but same length, we assume same ordering x1..xn or user will map outside.
            // (For UI, ensure both use same variable names, e.g., x1..xn.)
        }

        // Presets: returns description + TableResult
        public static (string description, TableResult result) Preset_Number_n3_num11()
        {
            // n=3, num=11 example explained in docs. We produce TableResult.
            var desc = new StringBuilder();
            desc.AppendLine("Пресет: n=3, num=11 (decimal).");
            desc.AppendLine("Конвенция: индексы кортежей идут от 0 до 2^n-1; индекс k соответствует кортежу (x1..xn) — двоичное представление k, x1 — старший бит.");
            desc.AppendLine("Значение f(кортеж) = (num >> k) & 1 (LSB соответствует кортежу k=0).");
            desc.AppendLine("Для num=11 (десятичное) = 00001011(в 8 битах). Тогда значения для кортежей 000..111: [1,1,0,1,0,0,0,0].");

            var res = GenerateFromNumber(3, 11);
            return (desc.ToString(), res);
        }

        public static (string description, TableResult result) Preset_Implication()
        {
            var desc = "Пресет: формула (x1 | x2) -> x3. Импликация разворачивается в логике как (! (x1 | x2)) | x3, но парсер поддерживает -> напрямую.";
            var res = GenerateFromFormula("(x1 | x2) -> x3");
            return (desc, res);
        }

        public static (string description, TableResult result) Preset_EquivalenceCheck()
        {
            var desc = "Пресет: сравнение (x1 & !x2) | x3 с её DNF. Будем генерировать DNF для первой, и сравнивать с исходной.";
            var f = "(x1 & !x2) | x3";
            var first = GenerateFromFormula(f);
            var dnf = first.DNF;
            var descFull = desc + Environment.NewLine + "Исходная формула: " + f + Environment.NewLine + "Её (сырая) DNF: " + dnf;
            var result = CompareFormulas(f, dnf);
            // But CompareFormulas expects formulas as strings; the DNF uses symbols ! & | which parser supports.
            var equal = result.equal;
            var r = equal ? ("Эквивалентны", first) : ("НЕ эквивалентны", first);
            return (descFull, first);
        }
    }

    #endregion
}
