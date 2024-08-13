using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Api.DTO.MyAccount
{
    public class GithubRepositoryDto
    {
        public string Name { get; set; }
        public string GitUrl { get; set; }
        public string CloneUrl { get; set; }
    }
}
