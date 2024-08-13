using DNDocs.Application.Shared;
using DNDocs.Api.DTO.ProjectManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Queries.ProjectManage
{
    public class GetProjectVersioningQuery : Query<ProjectVersioningDto>
    {
        public int Id { get; set; }
    }
}
