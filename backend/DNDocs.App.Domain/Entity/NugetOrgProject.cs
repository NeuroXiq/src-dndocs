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
        public NugetOrgProjectState State { get; set; }

        public DateTime? BuildStartOn { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime LastModifiedOn { get; set; }

        public int NugetPackageId { get; set; }

        public NugetPackage NugetPackage { get; set; }

        public NugetOrgProject() { }

        public NugetOrgProject(NugetPackage nugetPackage, NugetOrgProjectState state)
        {
            NugetPackage = nugetPackage;
            State = state;
        }
    }
}
