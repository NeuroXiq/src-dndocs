using Microsoft.EntityFrameworkCore;
using DNDocs.Application.Queries.ProjectManage;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Api.DTO.ProjectManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.QueryHandlers.ProjectManage
{
    internal class GetProjectVersioningHandler : QueryHandlerA<GetProjectVersioningQuery, ProjectVersioningDto>
    {
        private IAppUnitOfWork uow;
        private ICurrentUser user;

        public GetProjectVersioningHandler(IAppUnitOfWork uow,  ICurrentUser user)
        {
            this.uow = uow;
            this.user = user;
        }

        protected override async Task<ProjectVersioningDto> Handle(GetProjectVersioningQuery query)
        {
            var r = await uow.GetSimpleRepository<ProjectVersioning>().GetByIdCheckedAsync(query.Id);
            user.Forbidden(user.UserIdAuthorized!= r.UserId);
            var nugetPkgs = await uow.GetSimpleRepository<NugetPackage>().Query().Where(t => t.ProjectVersioningId == r.Id).ToListAsync();

            return Mapper.Map(r);
        }
    }
}
