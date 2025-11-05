using ClassLibraryBoolTable.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryBoolTable.Service
{
    /// <summary>
    /// Сравнивает две булевые функции по таблицам истинности.
    /// </summary>
    public static class FunctionComparer
    {
        /// <summary>
        /// Проверяет эквивалентность двух таблиц истинности.
        /// Возвращает true, если функции идентичны, иначе false и первое контр-слово.
        /// </summary>
        /// <param name="table1">Первая таблица</param>
        /// <param name="table2">Вторая таблица</param>
        /// <param name="counterExample">Первый кортеж, на котором функции различаются (или null, если эквивалентны)</param>
        public static bool AreEquivalent(
            List<TupleResult> table1,
            List<TupleResult> table2,
            out bool[] counterExample)
        {
            counterExample = null;

            if (table1.Count != table2.Count)
                throw new ArgumentException("Таблицы должны иметь одинаковое количество строк.");

            for (int i = 0; i < table1.Count; i++)
            {
                if (table1[i].Result != table2[i].Result)
                {
                    counterExample = table1[i].Variables;
                    return false;
                }
            }

            return true;
        }
    }
}
