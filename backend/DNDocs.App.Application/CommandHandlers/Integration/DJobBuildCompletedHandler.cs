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

            var project = await uow.ProjectRepository.GetByIdCheckedAsync(projectId);

            project.LastBuildCompletedOn = DateTime.UtcNow;

            if (cmd.Success)
            {
                project.State = Domain.Enums.ProjectState.Active;
                project.StateDetails = Domain.Enums.ProjectStateDetails.Ready;
            }
            else
            {
                project.State = Domain.Enums.ProjectState.NotActive;
                project.StateDetails = Domain.Enums.ProjectStateDetails.BuildFailed;
                project.LastBuildErrorLog = cmd.Exception;
            }
        }
    }
}
