using DNDocs.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Domain.Exceptions
{
    public class UnauthorizedException : RobiniaException
    {
        public UnauthorizedException()
        {
        }

        public UnauthorizedException(string msg) : base(msg) { }
    }
}
