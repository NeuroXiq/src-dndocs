using DNDocs.Application.Queries;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Api.DTO;
using DNDocs.Api.DTO.ProjectManage;

namespace DNDocs.Application.QueryHandlers
{
    internal class GetMyProjectsHandler : QueryHandler<GetMyProjectsQuery, IList<ProjectDto>>
    {
        private IAppUnitOfWork appUow;
        private int userid;

        public GetMyProjectsHandler(
            IAppUnitOfWork appUow,
            ICurrentUser currentUser)
        {
            this.appUow = appUow;
            this.userid = currentUser.UserIdAuthorized;
        }

        protected override IList<ProjectDto> Handle(GetMyProjectsQuery query)
        {
            var r = this.appUow.GetSimpleRepository<RefUserProject>()
                .Query()
                .Where(t => t.UserId == userid)
                .Select(t => t.Project)
                .Select(Mapper.Map)
                .ToList();

            return r;
        }
    }
}
