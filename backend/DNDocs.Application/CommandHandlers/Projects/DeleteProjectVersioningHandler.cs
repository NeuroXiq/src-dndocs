using Microsoft.EntityFrameworkCore;
using DNDocs.Application.Commands.Projects;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Repository;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.CommandHandlers.Projects
{
    internal class DeleteProjectVersioningHandler : CommandHandlerA<DeleteProjectVersioningCommand>
    {
        private ICurrentUser user;
        private IAppUnitOfWork uow;
        private IRepository<ProjectVersioning> projectVersioningRepo;
        private IProjectRepository projectRepo;
        private IRepository<NugetPackage> nugetPackageRepo;

        public DeleteProjectVersioningHandler(IAppUnitOfWork uow,
            ICurrentUser user)
        {
            this.user = user;
            this.uow = uow;
            this.projectVersioningRepo = uow.GetSimpleRepository<ProjectVersioning>();
            this.projectRepo = uow.ProjectRepository;
            this.nugetPackageRepo = uow.GetSimpleRepository<NugetPackage>();
        }

        public override async Task Handle(DeleteProjectVersioningCommand command)
        {
            var versioning = await this.projectVersioningRepo.GetByIdCheckedAsync(command.ProjectVersioningId);
            user.Forbidden(versioning.UserId != user.UserIdAuthorized);

            // load dependent entities (to delete them)
            await nugetPackageRepo.Query().Where(t => t.ProjectVersioningId == versioning.Id).ToListAsync();
            await projectRepo.Query().Where(t => t.PVProjectVersioningId == versioning.Id).ToListAsync();
            await uow.GetSimpleRepository<SystemMessage>().Query().Where(t => t.ProjectVersioningId == versioning.Id).ToListAsync();

            projectVersioningRepo.Delete(versioning);
        }
    }
}
