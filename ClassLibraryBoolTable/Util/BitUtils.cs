using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryBoolTable.Util
{
    /// <summary>
    /// Вспомогательные методы для работы с битами и булевыми функциями.
    /// </summary>
    public static class BitUtils
    {
        /// <summary>
        /// Генерирует все комбинации значений для n переменных.
        /// </summary>
        /// <param name="n">Количество переменных</param>
        /// <returns>Список массивов bool длиной n</returns>
        public static List<bool[]> GenerateAllTuples(int n)
        {
            int count = 1 << n; // 2^n
            var tuples = new List<bool[]>(count);

            for (int i = 0; i < count; i++)
            {
                bool[] tuple = new bool[n];
                for (int j = 0; j < n; j++)
                    tuple[n - j - 1] = ((i >> j) & 1) == 1;

                tuples.Add(tuple);
            }

            return tuples;
        }

        /// <summary>
        /// Декодирует номер булевой функции num в массив значений длиной 2^n.
        /// </summary>
        /// <param name="n">Количество переменных</param>
        /// <param name="num">Номер функции (0..2^(2^n)-1)</param>
        /// <returns>bool[] — значения функции для всех 2^n наборов</returns>
        public static bool[] DecodeFunction(int n, long num)
        {
            int length = 1 << n;
            bool[] values = new bool[length];

            for (int i = 0; i < length; i++)
                values[length - i - 1] = ((num >> i) & 1) == 1;

            return values;
        }

        /// <summary>
        /// Преобразует массив булевых значений в двоичную строку (для пояснений GUI).
        /// </summary>
        public static string ToBinaryString(bool[] bits)
        {
            char[] chars = new char[bits.Length];
            for (int i = 0; i < bits.Length; i++)
                chars[i] = bits[i] ? '1' : '0';
            return new string(chars);
        }
    }
}
