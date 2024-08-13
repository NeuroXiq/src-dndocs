using DNDocs.Api.DTO.MyAccount;
using DNDocs.Application.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Queries.Integration
{
    public class GetNugetCreateProjectStatusQuery : Query<BgJobViewModel>
    {
        public string PackageName { get; set; }
        public string PackageVersion { get; set; }
    }
}
