using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Api.DTO.Integration
{
    public class NugetGenerateProjectModel
    {
        public string PackageName { get; set; }
        public string PackageVersion { get; set; }
    }
}
