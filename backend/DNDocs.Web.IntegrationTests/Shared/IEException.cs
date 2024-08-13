using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.IntegrationTests.Shared
{
    internal class IEException : Exception
    {
        public IEException(string msg) : base(msg) { }
    }
}
