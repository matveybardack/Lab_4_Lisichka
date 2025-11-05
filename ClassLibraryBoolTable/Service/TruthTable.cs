using ClassLibraryBoolTable.Models;
using ClassLibraryBoolTable.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryBoolTable.Service
{
    /// <summary>
    /// Представляет таблицу истинности булевой функции.
    /// </summary>
    public class TruthTable
    {
        public List<string> VariableNames { get; }
        public List<TupleResult> Table { get; private set; }

        /// <summary>
        /// Создание таблицы для n переменных с именами x1, x2, ..., xn
        /// </summary>
        public TruthTable(int n)
        {
            VariableNames = Enumerable.Range(1, n).Select(i => $"x{i}").ToList();
            Table = new List<TupleResult>();
        }

        /// <summary>
        /// Генерация таблицы истинности из номера функции (num).
        /// </summary>
        public void BuildFromNumber(long num)
        {
            int n = VariableNames.Count;
            var tuples = BitUtils.GenerateAllTuples(n);
            var values = BitUtils.DecodeFunction(n, num);

            Table = tuples.Select((t, i) => new TupleResult(t, values[i])).ToList();
        }

        /// <summary>
        /// Генерация таблицы истинности из функции-делегата.
        /// Делегат получает массив bool (значения переменных), возвращает bool (результат функции).
        /// </summary>
        public void BuildFromFunction(Func<bool[], bool> func)
        {
            int n = VariableNames.Count;
            var tuples = BitUtils.GenerateAllTuples(n);

            Table = tuples.Select(t => new TupleResult(t, func(t))).ToList();
        }

        /// <summary>
        /// Возвращает строковое представление таблицы (для консоли/GUI).
        /// </summary>
        public string ToTableString()
        {
            var header = string.Join(" ", VariableNames) + " | f";
            var rows = Table.Select(tr => string.Join(" ", tr.Variables.Select(v => v ? "1" : "0")) + " | " + (tr.Result ? "1" : "0"));
            return header + Environment.NewLine + string.Join(Environment.NewLine, rows);
        }
    }
}
