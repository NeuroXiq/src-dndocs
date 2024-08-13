using DNDocs.Application.Shared;
using DNDocs.Api.DTO.ProjectManage;
using DNDocs.Api.DTO.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Queries.ProjectManage
{
    public class GetProjectsVersioningInfoQuery : Query<TableDataDto<ProjectVersioningInfoDto>>
    {
        public int ProjectVersioningId { get; set; }
        public int PageNo { get; set; }
    }
}
