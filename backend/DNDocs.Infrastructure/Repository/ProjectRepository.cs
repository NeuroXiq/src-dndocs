
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Repository;
using DNDocs.Domain.ValueTypes;
using DNDocs.Infrastructure.DataContext;
using Microsoft.EntityFrameworkCore;

namespace DNDocs.Infrastructure.Repository
{
    internal class ProjectRepository : BaseRepository<Project>, IProjectRepository
    {
        public ProjectRepository(AppDbContext dbcontext) : base(dbcontext)
        {
        }

        public async Task<Project> GetByNameAsync(string projectName)
        {
            return await dbset.Where(t => t.ProjectName == projectName).FirstOrDefaultAsync();
        }

        public Project GetByRobiniaUrlPrefix(string urlPrefix)
        {
            return dbset.Where(t => t.UrlPrefix == urlPrefix).FirstOrDefault();
        }

        public async Task<Project> GetNugetOrgProjectAsync(string packageName, string packageVersion)
        {
            return await dbset.Where(t => t.ProjectType == ProjectType.NugetOrg && 
                t.NugetOrgPackageName == packageName && 
                t.NugetOrgPackageVersion == packageVersion)
                .FirstOrDefaultAsync();
        }
    }
}
