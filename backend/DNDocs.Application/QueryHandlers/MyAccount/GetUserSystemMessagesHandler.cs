using DNDocs.Application.Queries.MyAccount;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Api.DTO.Shared;

namespace DNDocs.Application.QueryHandlers
{
    internal class GetUserSystemMessagesHandler : QueryHandler<GetUserSystemMessagesQuery, TableDataDto<SystemMessageDto>>
    {
        private ICurrentUser user;
        private IAppUnitOfWork appuow;

        public GetUserSystemMessagesHandler(ICurrentUser user, IAppUnitOfWork appuow)
        {
            this.user = user;
            this.appuow = appuow;
        }

        protected override TableDataDto<SystemMessageDto> Handle(GetUserSystemMessagesQuery query)
        {
            Validation.ThrowError(query.PageNo < 0, "PageNo < 0");
            Validation.ThrowError(query.RowsPerPage < 0 || query.RowsPerPage > 50, "RowsPerPage must be in range from 0 to 50");

            var dbquery = appuow.GetSimpleRepository<SystemMessage>().Query();
            var userid = user.UserIdAuthorized;

            if (query.ProjectId.HasValue)
            {
                user.AuthorizationProjectManage(query.ProjectId.Value);
                dbquery = dbquery.Where(t => t.ProjectId == query.ProjectId.Value);
            }
            else if (query.ProjectVersioningId.HasValue)
            {
                dbquery = dbquery.Where(t => t.ProjectVersioning.UserId == userid && t.ProjectVersioningId == query.ProjectVersioningId.Value);
            }
            else
            {
                dbquery = dbquery.Where(t =>
                    (t.UserId == userid) ||
                    t.Project.RefUserProject.Any(rup => rup.UserId == userid) ||
                    t.ProjectVersioning.UserId == userid);
            }

            var totalCount = dbquery.Count();

            var messages = dbquery
                .OrderByDescending(t => t.DateTime)
                .Skip(query.PageNo * query.RowsPerPage)
                .Take(query.RowsPerPage)
                .ToList();

            var smDtos = Mapper.Map(messages);

            return new TableDataDto<SystemMessageDto>(totalCount, query.PageNo, query.RowsPerPage, smDtos);
        }
    }
}
