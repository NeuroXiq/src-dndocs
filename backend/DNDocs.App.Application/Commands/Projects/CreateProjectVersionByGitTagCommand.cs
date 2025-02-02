using DNDocs.Application.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Commands.Projects
{
    public class CreateProjectVersionByGitTagCommand : Command<int>, ICommandBgJob
    {
        public int ProjectVersioningId { get; set; }
        public string GitTagName { get; set; }
    }
}
