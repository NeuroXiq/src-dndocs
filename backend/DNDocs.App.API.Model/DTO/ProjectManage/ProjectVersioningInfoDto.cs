using DNDocs.Api.DTO.Enums;
using DNDocs.Api.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Api.DTO.ProjectManage
{
    public class ProjectVersioningInfoDto
    {
        public string GitTagName { get; set; }
        public int? ProjectId { get; set; }
        public IList<NugetPackageDto> ProjectNugetPackages { get; set; }

        public ProjectVersioningInfoDto() { }

        public ProjectVersioningInfoDto(string gitTag, int? projectId, IList<NugetPackageDto> projectNugetPackages)
        {
            GitTagName = gitTag;
            ProjectId = projectId;
            ProjectNugetPackages = projectNugetPackages;
        }
    }
}
