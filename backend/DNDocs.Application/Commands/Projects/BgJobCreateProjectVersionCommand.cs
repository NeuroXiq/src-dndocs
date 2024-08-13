using DNDocs.Application.Shared;
using DNDocs.Domain.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Commands.Projects
{
    internal class BgJobCreateProjectVersionCommand : CreateProjectVersionCommand, ICommandBgJob
    {
        public BgJobCreateProjectVersionCommand() { }

        public BgJobCreateProjectVersionCommand(int projectVersioningId, string gitTagName, IList<NugetPackageDto> nugetPackages)
        {
            ProjectVersioningId = projectVersioningId;
            GitTagName = gitTagName;
            NugetPackages = nugetPackages;
        }
    }
}
