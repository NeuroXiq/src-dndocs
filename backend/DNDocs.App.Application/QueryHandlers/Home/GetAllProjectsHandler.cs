using DNDocs.Application.Queries.Home;
using DNDocs.Application.Shared;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Api.DTO;
using DNDocs.Api.DTO.ProjectManage;

namespace DNDocs.Application.QueryHandlers.Home
{
    internal class GetAllProjectsHandler : QueryHandler<GetAllProjectsQuery, IList<ProjectDto>>
    {
        private IAppUnitOfWork appuow;

        public GetAllProjectsHandler(IAppUnitOfWork appuow)
        {
            this.appuow = appuow;
        }


        protected override IList<ProjectDto> Handle(GetAllProjectsQuery query)
        {
            var r = appuow.ProjectRepository.Query().Where(t => t.State == Domain.Enums.ProjectState.Active).ToList();

            var rm = r.Select(t => Mapper.Map(t)).ToList();

            return rm;
        }
    }
}
