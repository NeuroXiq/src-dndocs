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
    internal class GetAllProjectVersioningHandler : QueryHandlerA<GetAllProjectVersioningQuery, IList<ProjectVersioningDto>>
    {
        private IAppUnitOfWork uow;
        private ICurrentUser user;

        public GetAllProjectVersioningHandler(
            ICurrentUser user,
            IAppUnitOfWork uow)
        {
            this.uow = uow;
            this.user = user;
        }

        protected override async Task<IList<ProjectVersioningDto>> Handle(GetAllProjectVersioningQuery query)
        {
            user.Authorize();
            var id = user.UserIdAuthorized;

            var versionings = await uow.GetSimpleRepository<ProjectVersioning>().Query().Where(t => t.UserId == id).ToListAsync();
            return Mapper.Map(versionings);
        }
    }
}
