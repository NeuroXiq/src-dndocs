using DNDocs.Application.Shared;
using DNDocs.Api.DTO;
using DNDocs.Api.DTO.ProjectManage;

namespace DNDocs.Application.Queries
{
    public class GetProjectByIdQuery : Query<ProjectDto>
    {
        public int Id { get; set; }

        public GetProjectByIdQuery() { }

        public GetProjectByIdQuery(int id)
        {
            Id = id;
        }
    }
}
