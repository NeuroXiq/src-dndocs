using DNDocs.Application.Commands.Integration;
using DNDocs.Application.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.CommandHandlers.Integration
{
    internal class DJobBuildCompletedHandler : CommandHandlerA<DJobBuildCompletedCommand>
    {
        public override async Task Handle(DJobBuildCompletedCommand cmd)
        {
            int projectId = -1;

            checked
            {
                // should never happend, inconsistent long id / int id acoss projects, neeed change to long/int forall projects
                projectId = (int)cmd.ProjectId;
            }

            var project = await uow.NugetOrgProjectRepository.GetByIdCheckedAsync(projectId);

            if (cmd.Success)
            {
                project.State = Domain.Enums.NugetOrgProjectState.Online;
            }
            else
            {
                project.State = Domain.Enums.NugetOrgProjectState.BuildFailed;
            }
        }
    }
}
