using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.Exceptions
{
    public class VValidationException : Exception
    {
        public VValidationException(string message) : base(message) { }
    }
}
