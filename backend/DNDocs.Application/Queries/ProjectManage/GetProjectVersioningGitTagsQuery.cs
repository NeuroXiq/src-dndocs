using DNDocs.Application.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Queries.ProjectManage
{
    public class GetProjectVersioningGitTagsQuery : Query<string[]>
    {
        public int ProjectVersioningId { get; set; }
    }
}
