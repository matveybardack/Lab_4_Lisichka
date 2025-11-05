using ClassLibraryBoolTable.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryBoolTable.Service
{
    /// <summary>
    /// Генератор KNF для булевой функции.
    /// </summary>
    public static class KnfGenerator
    {
        /// <summary>
        /// Создаёт KNF в базисе {!, &, |} по таблице истинности.
        /// </summary>
        /// <param name="table">Таблица истинности</param>
        /// <param name="variableNames">Имена переменных (x1..xn)</param>
        /// <returns>Строка с KNF</returns>
        public static string BuildKNF(List<TupleResult> table, List<string> variableNames)
        {
            var clauses = new List<string>();

            foreach (var row in table)
            {
                if (row.Result) continue; // берем только строки, где f=0

                var literals = new List<string>();
                for (int i = 0; i < variableNames.Count; i++)
                {
                    // Для KNF: true → !xi, false → xi (инверсный порядок по сравнению с DNF)
                    literals.Add(row.Variables[i] ? "!" + variableNames[i] : variableNames[i]);
                }

                clauses.Add("(" + string.Join(" | ", literals) + ")");
            }

            if (!clauses.Any())
                return "1"; // константа 1
            return string.Join(" & ", clauses);
        }
    }
}
