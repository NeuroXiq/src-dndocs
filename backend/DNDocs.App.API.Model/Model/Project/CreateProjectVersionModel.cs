using DNDocs.Api.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Api.Project
{
    public class CreateProjectVersionModel
    {
        public int ProjectVersioningId { get; set; }
        public virtual string GitTagName { get; set; }
        public NugetPackageModel[] NugetPackages { get; set; }

        public CreateProjectVersionModel() { }
        public CreateProjectVersionModel(int projectVersioningId, 
            string gitTagName,
            NugetPackageModel[] nugetPackages)
        {
            ProjectVersioningId = projectVersioningId;
            GitTagName = gitTagName;
            NugetPackages = nugetPackages;
        }
    }
}
