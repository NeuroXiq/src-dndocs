using DNDocs.Domain.Entity.App;
using DNDocs.Domain.ValueTypes;
using DNDocs.Domain.ValueTypes.Project;
using System.Runtime.CompilerServices;

namespace DNDocs.Domain.Service
{
    public interface IProjectManager
    {
        Task AutoupgradeSingleton(int projectid);
        Task DeleteProject(int id);
        string NewestMdDocsCommit(string repoUrl, string docsFolderPath, string readmePath);
        Task ValidateGitRepository(
           string gitRepoUrl,
           string gitBranchName,
           string gitHomepageRelativePath,
           string gitDocsRelativePath,
           [CallerArgumentExpression("gitRepoUrl")]string vGitRepoUrl = "",
           [CallerArgumentExpression("gitBranchName")]string vGitBranchName = "",
           [CallerArgumentExpression("gitHomepageRelativePath")]string vGitHomepageRelativePath = "",
           [CallerArgumentExpression("gitDocsRelativePath")] string vGitDocsRelativePath = ""
            );


        Task<int> CreateSingletonProject(CreateProjectParams p);
        //Task<int> CreateVersionProject(int projectVersioningId, string gitTagName, IList<NugetPackageDto> nugetPackages);
        //Task<int> CreateVersionProject(int projectVersioningId, string gitTagName);

        Task ValidateCreate(CreateProjectParams p, bool isRawUserInput =  false);

        Task<List<NugetPackage>> ValidateAndCreateNugetPackages(IList<NugetPackageDto> nugetPackages, bool fetchLatestPackageIfIdentityVersionNull);
        // Task ValidateCreateProjectVersion(int projectVersioningId, string gitTagName, IList<NugetPackageDto> nugetPackages);
        // Task AutoupgradeProjectVersion(int projectVersioningId);
    }
}
