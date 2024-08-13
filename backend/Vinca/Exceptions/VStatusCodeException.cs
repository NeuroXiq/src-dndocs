using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.Exceptions
{
    public class VStatusCodeException : Exception
    {
        public int StatusCode { get; set; }

        public VStatusCodeException(System.Net.HttpStatusCode unauthorized, string message) : base(message) { }
    }
}
