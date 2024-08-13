using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.Exceptions
{
    public class VAppException : Exception
    {
        public VAppException(string message) : base(message){ }
    }
}
