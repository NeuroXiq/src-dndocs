using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Domain.ValueTypes
{
    public class NugetPackageDto
    {
        public string IdentityId { get; set; }
        public string IdentityVersion { get; set; }

        public NugetPackageDto() { }
        public NugetPackageDto(string identityId, string identityVersion)
        {
            IdentityId = identityId;
            IdentityVersion = identityVersion;
        }
    }
}
