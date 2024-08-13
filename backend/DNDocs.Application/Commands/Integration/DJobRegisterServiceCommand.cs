using DNDocs.Application.Shared;
using DNDocs.Domain.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Commands.Integration
{
    public class DJobRegisterServiceCommand : Command
    {
        public string InstanceName { get; set; }
        public int ServerPort { get; set; }
        public string ServerIpAddress { get; set; }
    }
}
