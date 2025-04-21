using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Storage_Common
{
    public class DataValidationException : Exception
    {
        public DataValidationException(string message)
            : base(message) { }
    }

    public static class DataValidators
    {
        public static void GreaterThan(int var, int value, string varName = "")
        {
            if (!(var > value))
            {
                throw new DataValidationException(
                    $"[[{varName}]] must be greater than [[{value}]]"
                );
            }
        }

        public static void GreaterThan(ulong var, ulong value, string varName = "")
        {
            if (!(var > value))
            {
                throw new DataValidationException(
                    $"[[{varName}]] must be greater than [[{value}]]"
                );
            }
        }
    }
}
