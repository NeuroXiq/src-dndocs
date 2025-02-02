using DNDocs.Application.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Commands.Integration
{
    public class NugetCreateProjectCommand : Command
    {
        public string PackageName { get; set; }
        public string PackageVersion { get; set; }
    }
}
