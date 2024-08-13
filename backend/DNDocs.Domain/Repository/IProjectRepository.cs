using DNDocs.Domain.Entity.App;

namespace DNDocs.Domain.Repository
{
    public interface IProjectRepository : IRepository<Project>
    {
        Task<Project> GetByNameAsync(string projectName);
        Project GetByRobiniaUrlPrefix(string projectName);
        Task<Project> GetNugetOrgProjectAsync(string packageName, string packageVersion);
    }
}
