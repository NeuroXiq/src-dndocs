using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Job.Api.Management
{
    public class BuildNugetOrgProjectModel
    {
        public long ProjectId { get; set; }
        public string PackageName { get; set; }
        public string PackageVersion { get; set; }
    }
}
