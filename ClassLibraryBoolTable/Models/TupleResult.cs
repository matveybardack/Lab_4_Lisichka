using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryBoolTable.Models
{
    /// <summary>
    /// Представляет одну строку таблицы истинности.
    /// Например: x1=0, x2=1, x3=1 => f=0
    /// </summary>
    public class TupleResult
    {
        public bool[] Variables { get; }
        public bool Result { get; set; }

        public TupleResult(bool[] variables, bool result)
        {
            Variables = variables;
            Result = result;
        }

        /// <summary>
        /// Возвращает строковое представление, например: "0 1 1 | 0"
        /// </summary>
        public override string ToString()
        {
            string vars = string.Join(" ", Variables.Select(v => v ? "1" : "0"));
            return $"{vars} | {(Result ? "1" : "0")}";
        }
    }
}
