using DNDocs.Application.Commands.Projects;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Repository;
using DNDocs.Domain.Service;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Domain.ValueTypes.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.CommandHandlers.Projects
{
    internal class BgJobCreateProjectVersionHandler : CommandHandlerA<BgJobCreateProjectVersionCommand, int>
    {
        private IProjectManager projectManager;

        public BgJobCreateProjectVersionHandler(IProjectManager projectManager)
        {
            this.projectManager = projectManager;
        }

        public override async Task<int> Handle(BgJobCreateProjectVersionCommand command)
        {
            throw new Exception();
            //return await projectManager.CreateVersionProject(
            //    command.ProjectVersioningId,
            //    command.GitTagName,
            //    command.NugetPackages.ToArray() ?? new Domain.ValueTypes.DTO.NugetPackageDto[0]);
        }
    }
}
