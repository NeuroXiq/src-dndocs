using DNDocs.Application.Shared;
using DNDocs.Domain.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Commands.Integration
{
    public class DJobBuildCompletedCommand : Command
    {
        public long ProjectId { get; set; }
        public bool Success { get; set; }
        public string Exception { get; set; }
    }
}
