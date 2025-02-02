using DNDocs.Application.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Commands.Projects
{
    public class DeleteProjectVersioningCommand : Command
    {
        public int ProjectVersioningId { get; set; }
    }
}
