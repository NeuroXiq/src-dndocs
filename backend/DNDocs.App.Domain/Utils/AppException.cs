using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Domain.Utils
{
    public class AppException : DNDomainException
    {
        public AppException(string msg) : base(msg) { }
    }
}
