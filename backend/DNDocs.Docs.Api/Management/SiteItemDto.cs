using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Docs.Api.Management
{
    public class SiteItemDto
    {
        public long Id { get; set; }
        public long DDocsProjectId { get; set; }
        public string Path { get; set; }
        public string FullUri { get; set; }

        public SiteItemDto() { }

        public SiteItemDto(long id, long ddocsProjectId, string path, string fullUri)
        {
            Id = id;
            DDocsProjectId = ddocsProjectId;
            Path = path;
            FullUri = fullUri;
        }
    }
}
