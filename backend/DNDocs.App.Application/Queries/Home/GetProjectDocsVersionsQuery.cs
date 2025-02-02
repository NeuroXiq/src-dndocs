using DNDocs.Application.Shared;
using DNDocs.Api.DTO.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Queries.Home
{
    public class GetProjectDocsVersionsQuery : Query<IList<ProjectDocsVersionDto>>
    {
        public int ProjectVersioningId { get; set; }
    }
}
