using DNDocs.Application.Shared;
using DNDocs.Api.DTO;
using DNDocs.Api.DTO.ProjectManage;

namespace DNDocs.Application.Queries.Home
{
    public class GetAllProjectsQuery : Query<IList<ProjectDto>>
    {
        public GetAllProjectsQuery()
        {
        }
    }
}
