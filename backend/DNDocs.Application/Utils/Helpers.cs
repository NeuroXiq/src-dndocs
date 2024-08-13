using DNDocs.Domain.Entity.App;
using DNDocs.Shared.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Utils
{
    internal class Helpers
    {
        public static string GetProjectUrl(Project project, DNDocsSettings settings)
        {
            switch (project.ProjectType)
            {
                case ProjectType.Singleton: return settings.GetUrlSingletonProject(project.UrlPrefix);
                case ProjectType.Version: return settings.GetUrlVersionProject(project.UrlPrefix, project.PVGitTag);
                case ProjectType.NugetOrg: return settings.GetUrlNugetOrgProject(project.NugetOrgPackageName, project.NugetOrgPackageVersion);
                default:
                    throw new NotImplementedException($"not implemented url for project type {project.ProjectType}");
            }
        }
    }
}
