using DNDocs.Domain.Entity;
using DNDocs.Domain.Entity.Shared;
using DNDocs.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.App.Domain.Entity
{
    public class NugetOrgProject : EntityBase, ICreateUpdateTimestamp
    {
        public string PackageName { get; set; }

        public string PackageVersion { get; set; }

        public bool IsOnline { get; set; }

        public NugetOrgProjectState State { get; set; }

        public DateTime? BuildStartOn { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime LastModifiedOn { get; set; }

        public NugetOrgProject() { }
        
        public NugetOrgProject(string packageName, string packageVersion)
        {
            PackageName = packageName;
            PackageVersion = packageVersion;
        }
    }
}
