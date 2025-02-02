using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.Exceptions
{
    /// <summary>
    /// Vinca validation
    /// </summary>
    public class VValidate
    {
        public static void AppEx(string msg) => AppEx(true, msg);

        public static void AppEx(bool tthorw, string msg)
        {
            if (tthorw) throw new VAppException(msg);
        }

        public static void ThrowError(bool tthrow, string message)
        {
            if (tthrow) Throw(message);
        }

        public static void Throw(bool shouldThrow, string message)
        {
            if (shouldThrow) Throw(message);
        }

        public static void EnumDefined<T>(T value) where T: struct, Enum
        {
            if (!Enum.IsDefined<T>(value)) { Throw($"'{typeof(T).Name}' enum value is not defined. Current value: '{value}'"); }
        }

        static void Throw(string msg) => throw new VValidationException(msg);
    }
}
