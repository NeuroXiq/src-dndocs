using DNDocs.Application.Shared;
using DNDocs.Domain.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Commands.Projects
{
    public class CreateProjectVersionCommand : Command<int>
    {
        public int ProjectVersioningId { get; set; }
        public string GitTagName { get; set; }
        public IList<NugetPackageDto> NugetPackages { get; set; }
    }
}
