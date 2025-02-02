using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Api.DTO.Home
{
    public class ProjectDocsVersionDto
    {
        public string ProjectUrlPrefix { get; set; }
        public string GitTagName { get; set; }
    }
}
