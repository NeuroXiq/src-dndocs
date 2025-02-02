using DNDocs.Domain.Entity;
using DNDocs.Domain.Entity.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.App.Domain.Entity
{
    public class NugetProject : EntityBase, ICreateUpdateTimestamp
    {
        public string PackageName { get; set; }

        public string PackageVersion { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime LastModifiedOn { get; set; }
    }
}
