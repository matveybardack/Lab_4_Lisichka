using ClassLibraryBoolTable.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryBoolTable.Service
{
    /// <summary>
    /// Генератор DNF для булевой функции.
    /// </summary>
    public static class DnfGenerator
    {
        /// <summary>
        /// Создаёт DNF в базисе {!, &, |} по таблице истинности.
        /// </summary>
        /// <param name="table">Таблица истинности</param>
        /// <param name="variableNames">Имена переменных (x1..xn)</param>
        /// <returns>Строка с DNF</returns>
        public static string BuildDNF(List<TupleResult> table, List<string> variableNames)
        {
            var clauses = new List<string>();

            foreach (var row in table)
            {
                if (!row.Result) continue; // берем только строки, где f=1

                var literals = new List<string>();
                for (int i = 0; i < variableNames.Count; i++)
                {
                    literals.Add(row.Variables[i] ? variableNames[i] : "!" + variableNames[i]);
                }

                clauses.Add("(" + string.Join(" & ", literals) + ")");
            }

            if (!clauses.Any())
                return "0"; // константа 0
            return string.Join(" | ", clauses);
        }
    }
}
