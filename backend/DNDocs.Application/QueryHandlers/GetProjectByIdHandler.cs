using Microsoft.EntityFrameworkCore;
using DNDocs.Application.Queries;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Api.DTO;
using DNDocs.Api.DTO.ProjectManage;

namespace DNDocs.Application.QueryHandlers
{
    internal class GetProjectByIdHandler : QueryHandlerA<GetProjectByIdQuery, ProjectDto>
    {
        private ICurrentUser currentUser;
        private IAppUnitOfWork appuow;

        public GetProjectByIdHandler(
            ICurrentUser currentUser,
            IAppUnitOfWork appuow)
        {
            this.currentUser = currentUser;
            this.appuow = appuow;
        }

        protected override async Task<ProjectDto> Handle(GetProjectByIdQuery query)
        {
            this.currentUser.AuthorizationProjectManage(query.Id);
            var p = this.appuow.GetSimpleRepository<Project>().GetById(query.Id);
            await appuow.GetSimpleRepository<NugetPackage>().Query().Where(t => t.ProjectId == p.Id).ToListAsync();

            return Mapper.Map(p);
        }
    }
}
