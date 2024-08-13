using DNDocs.Application.Queries.Home;
using DNDocs.Application.Shared;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Api.DTO;
using DNDocs.Api.DTO.ProjectManage;

namespace DNDocs.Application.QueryHandlers.Home
{
    internal class GetRecentProjectsHandler : QueryHandler<GetRecentProjectsQuery, IList<ProjectDto>>
    {
        private IAppUnitOfWork appuow;

        public GetRecentProjectsHandler(IAppUnitOfWork appuow)
        {
            this.appuow = appuow;
        }

        protected override IList<ProjectDto> Handle(GetRecentProjectsQuery query)
        {
            var r = appuow.ProjectRepository.Query()
                .Where(t => t.State == Domain.Enums.ProjectState.Active)
                .Take(12)
                .ToList();

            var mr = r.Select(a => Mapper.Map(a)).ToList();

            return mr;
        }
    }
}
