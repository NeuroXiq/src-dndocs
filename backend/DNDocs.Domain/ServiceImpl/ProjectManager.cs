using Microsoft.Extensions.Logging;
using DNDocs.Shared.Log;
using Microsoft.Extensions.Options;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Enums;
using DNDocs.Domain.Service;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Domain.ValueTypes;
using DNDocs.Domain.ValueTypes.Project;
using DNDocs.Shared.Configuration;
using DNDocs.Shared.Utils;
using System.Linq.Expressions;
using System.Text;
using System.Runtime.CompilerServices;
using DNDocs.Domain.ValueTypes;
using DNDocs.Domain.Repository;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using DNDocs.Docs.Api.Client;
using System.Reflection;
using System.Diagnostics;
using Vinca.Api.Nuget;

namespace DNDocs.Domain.ServiceImpl
{
    public class ProjectManager : IProjectManager
    {
        private IBlobDataService blobDataService;
        private IAppUnitOfWork appUow;
        private ICurrentUser currentUser;
        private DNDocsSettings roptions;
        private IUnitOfWorkFactory uowFactory;
        private IAppManager appManager;
        private IRepository<ProjectVersioning> projectVersioningRepo;
        private INugetRepositoryFacade nugetRepositoryFacade;
        private ISystemMessages systemMessages;
        private ICurrentUser user;
        private IProjectRepository projectRepo;
        private ILog<ProjectManager> logger;
        const int MaxProjectsPerUser = 50;

        public ProjectManager(
            IAppUnitOfWork uow,
            ICurrentUser currentUser,
            IBlobDataService blobDataService,
            IAppManager appManager,
            IUnitOfWorkFactory uowFactory,
            IOptions<DNDocsSettings> rsettings,
            ILog<ProjectManager> logger,
            ICurrentUser user,
            ISystemMessages systemMessages,
            INugetRepositoryFacade nugetRepositoryFacade)
        {
            this.nugetRepositoryFacade = nugetRepositoryFacade;
            this.systemMessages = systemMessages;
            this.user = user;
            this.logger = logger;
            this.blobDataService = blobDataService;
            this.currentUser = currentUser;
            appUow = uow;

            roptions = rsettings.Value;
            this.uowFactory = uowFactory;
            this.appManager = appManager;
            this.projectRepo = this.appUow.ProjectRepository;
            this.projectVersioningRepo = uow.GetSimpleRepository<ProjectVersioning>();
        }

        public async Task DeleteProject(int projectId)
        {
            var id = projectId;
            // user.AuthorizationProjectManage(id); // todo: reuired by nugetorg generator (delete if failed) what to do with this? param?? maybe just auth as nuget user
            // in nugetcreateproject handler? (to not be anonymous here?)
            var projectRepo = appUow.GetSimpleRepository<Project>();

            Validation.ThrowError(projectRepo.GetById(id) == null, $"Project with id: '{id}' does not exists");

            await appUow.GetSimpleRepository<RefUserProject>().ExecuteDeleteAsync(t => t.ProjectId == id);
            await appUow.GetSimpleRepository<NugetPackage>().ExecuteDeleteAsync(t => t.ProjectId == id);
            await appUow.GetSimpleRepository<SystemMessage>().ExecuteDeleteAsync(t => t.ProjectId == id);

            await projectRepo.DeleteAsync(id);
        }

        public async Task AutoupgradeSingleton(int projectId)
        {
            throw new NotImplementedException();
            //logger.LogTrace("starting autorebuild method");
            //user.AuthorizationProjectManage(projectId);
            //var project = appUow.GetSimpleRepository<Project>().Query(t => t.ProjectNugetPackages).Where(t => t.Id == projectId).Single();
            //Validation.ThrowError(project.PVProjectVersioningId.HasValue, "Cannot autoupgrade because this project is versioned");

            //bool mdDocsNeedUpdate = false;
            //bool nupkgNeedUpgrade = false;

            //// check if new nuget packages needed
            //var latestNugetPackages = new List<PackageSearchMetadata>();
            //var currentProjPackages = project.ProjectNugetPackages.ToList();
            //var curNupksString = currentProjPackages.StringJoin("\r\n", p => $"{p.IdentityId} {p.IdentityVersion}");

            //foreach (var currentProjPackage in currentProjPackages)
            //{
            //    var latest = nugetRepositoryFacade.GetLatestPackage(currentProjPackage.IdentityId);
            //    latestNugetPackages.Add(latest);
            //}

            //nupkgNeedUpgrade = !latestNugetPackages
            //    .All(latestPck => currentProjPackages
            //        .Exists(curpkc => curpkc.IdentityId == latestPck.IdentityId && curpkc.IdentityVersion == latestPck.IdentityVersion));

            //if (nupkgNeedUpgrade)
            //{
            //    var packagesToAdd = latestNugetPackages.Select(p =>
            //    new NugetPackage(
            //        p.Title,
            //        p.IdentityVersion,
            //        p.IdentityId,
            //        p.Published,
            //        p.ProjectUrl,
            //        p.PackageDetailsUrl,
            //        p.IsListed))
            //    .ToList();

            //    project.ProjectNugetPackages.Clear();
            //    project.ProjectNugetPackages.AddRange(packagesToAdd);
            //    project.NupkgAutorebuildLastDateTime = DateTime.UtcNow;

            //    systemMessages.Info(project,
            //        $"Auto-Rebuild Nuget Packages for project: {project}",
            //        $"Upgrade in database nuget packages completed\r\n" +
            //        $"Old Nuget Packages: \r\n" +
            //        $"{curNupksString}" +
            //        $"\r\nNew Nuget Packages\r\n: " +
            //        $"{latestNugetPackages.StringJoin("\r\n", p => $"{p.IdentityId} {p.IdentityVersion}")}");
            //}
            //else
            //{
            //    systemMessages.Info(
            //    project,
            //   "AutoUpdate nuget packages - already up to date",
            //   $"AutoUpdate project packages skip because all packages are up to date. Current project packages:\r\n {currentProjPackages.StringJoin("\r\n")}");
            //}

            //// check md docs need upgrade
            //if (!string.IsNullOrWhiteSpace(project.GitMdRepoUrl))
            //{
            //    var newestCommit = NewestMdDocsCommit(project.GitMdRepoUrl, project.GitMdRelativePathDocs, project.GitMdRelativePathReadme);
            //    mdDocsNeedUpdate = newestCommit != project.GitDocsCommitHash;
            //    project.GitDocsCommitHash = newestCommit;
            //}

            //if (!mdDocsNeedUpdate && !nupkgNeedUpgrade)
            //{
            //    systemMessages.Info(project,
            //        "No update needed",
            //        "Nuget packages and current commit hash does not require updated and rebuild (already up-to-date). Skip building project.\r\n");

            //    return;
            //}

            //throw new Exception("implement to queue this work");
            //// await BuildProject(project.Id);
        }

        public string NewestMdDocsCommit(string repoUrl, string docsFolderPath, string readmePath)
        {
            throw new NotImplementedException();
            // select newest commit from readme.md or docs folder

            //using (var git = appManager.OpenGitRepo(repoUrl))
            //{
            //    int no = 999999999;
            //    string newest = null;

            //    if (!string.IsNullOrWhiteSpace(readmePath))
            //    {
            //        newest = git.NewestCommitHashForPath(readmePath);
            //        no = git.GetCommitNoFromHEAD(newest);
            //    }

            //    if (!string.IsNullOrWhiteSpace(docsFolderPath))
            //    {
            //        string docsCommit = git.NewestCommitHashForPath(docsFolderPath);
            //        int docsNo = git.GetCommitNoFromHEAD(docsCommit);

            //        newest = no > docsNo ? docsCommit : newest;
            //    }

            //    return newest;
            //}
        }
        

        public async Task ValidateCreate(CreateProjectParams p, bool isRawUserInput = false)
        {
            //var urlPrefix = p.UrlPrefix?.Trim();
            //var userid = currentUser.UserIdAuthorized;

            //var projectsCount = appUow.GetSimpleRepository<RefUserProject>()
            //       .Query()
            //       .Where(t => t.UserId == currentUser.UserIdAuthorized)
            //       .Count();

            //// Validation.ThrowError(!string.IsNullOrWhiteSpace(p.DocfxTemplate) && !docfxManager.TemplateExists(p.DocfxTemplate), $"Template does not exists: '{p.DocfxTemplate}'");

            //var projectRepo = appUow.ProjectRepository;

            //var invalidSameNames = projectRepo.Query().Any(t => t.ProjectType == p.ProjectType && t.ProjectName == p.ProjectName);

            //Validation.ThrowFieldError(nameof(p.ProjectName), "ProjectNameUniqueAndNotEmpty", string.IsNullOrEmpty(p.ProjectName) || invalidSameNames);

            //if (p.ProjectType != ProjectType.NugetOrg)
            //{
            //    // var invalidSameUrlPrefix = projectRepo.Query().Any(t => t.ProjectType == p.ProjectType && p.UrlPrefix == t.UrlPrefix);
            //    // Project.ValidateUrlPrefix(p.UrlPrefix, nameof(p.UrlPrefix));
            //    // Validation.ThrowFieldError(nameof(p.UrlPrefix), "urlprefix is not unique in the system", invalidSameUrlPrefix);
            //}
            //else
            //{
            //    // Validation.ThrowFieldError(nameof(p.UrlPrefix), "Urlprefix for nuget proejct must be null", p.UrlPrefix != null);
            //}

            //await ValidateAndCreateNugetPackages(p.NugetPackages, false);

            //if (!string.IsNullOrWhiteSpace(p.GitMdRepoUrl))
            //{
            //    // skip because system will detect and set this later else (value unkonwn when user submits form)
            //    if (!isRawUserInput) Validation.NotStringIsNullOrWhiteSpace(p.GitDocsCommitHash);
            //    await ValidateGitRepository(p.GitMdRepoUrl, p.GitMdBranchName, p.GitMdRelativePathReadme, p.GitMdRelativePathDocs);
            //}
            //else
            //{
            //    var errMsg = "Must be empty if there is no github md docs repository";
            //    Validation.FieldErrorP(!string.IsNullOrEmpty(p.GitMdBranchName), p.GitMdBranchName, errMsg);
            //    Validation.FieldErrorP(!string.IsNullOrEmpty(p.GitMdRelativePathDocs), p.GitMdRelativePathDocs, errMsg);
            //    Validation.FieldErrorP(!string.IsNullOrEmpty(p.GitMdRelativePathReadme), p.GitMdRelativePathReadme, errMsg);
            //    Validation.FieldErrorP(!string.IsNullOrEmpty(p.GitDocsCommitHash), p.GitDocsCommitHash, errMsg);
            //}
        }

        public async Task ValidateGitRepository(
            string gitRepoUrl,
            string gitBranchName,
            string gitHomepageRelativePath,
            string gitDocsRelativePath,
            [CallerArgumentExpression("gitRepoUrl")] string vGitRepoUrl = "",
            [CallerArgumentExpression("gitBranchName")] string vGitBranchName = "",
            [CallerArgumentExpression("gitHomepageRelativePath")] string vGitHomepageRelativePath = "",
            [CallerArgumentExpression("gitDocsRelativePath")] string vGitDocsRelativePath = ""
            )
        {
            // todo maybe validate project.gitcommithash if exists?

            Validation.ThrowFieldError(vGitRepoUrl, "Repository does not exists", !appManager.GitRepoExistsURL(gitRepoUrl));
            Validation.ThrowFieldError(vGitRepoUrl,
                "If git repository is not empty at least one value: 'docs relative path' or 'readme.md path' must not be empty",
                string.IsNullOrWhiteSpace(gitHomepageRelativePath) && string.IsNullOrWhiteSpace(gitDocsRelativePath));

            //using (var git = appManager.OpenGitRepo(gitRepoUrl))
            //{
            //    Validation.ThrowError(!git.BranchExists(gitBranchName), "Branch does not exists");
            //    git.CheckoutBranch(gitBranchName);

            //    Validation.ThrowError(
            //        !string.IsNullOrWhiteSpace(gitHomepageRelativePath) &&
            //        !File.Exists(Path.Combine(git.RepoOSPath, gitHomepageRelativePath)),
            //        $"Readme file '{gitHomepageRelativePath}' does not exists (branch name? file name is case sensitive)");

            //    Validation.ThrowError(
            //        !string.IsNullOrWhiteSpace(gitDocsRelativePath) &&
            //        !Directory.Exists(Path.Combine(git.RepoOSPath, gitDocsRelativePath)),
            //        "Folder does not exists, (or not exists in provided branch name, file name is case sensitive)"
            //        );
            //}
        }

        private async Task<Project> InsertIntoDb(CreateProjectParams p)
        {
            var c = ValidateCreate(p);
            c.Wait();

            // Step 1 insert into database

            var urlPrefix = p.UrlPrefix?.Trim();

            var project = new Project(
                p.ProjectName,
                p.UrlPrefix,
                p.Description,
                p.GithubUrl,
                p.DocfxTemplate,
                Domain.Enums.ProjectState.NotActive,
                null,
                p.GitMdRepoUrl,
                p.GitMdBranchName,
                p.GitMdRelativePathDocs,
                p.GitMdRelativePathReadme,
                p.GitDocsCommitHash,
                p.PSAutorebuild,
                p.ProjectType,
                p.NugetOrgPackageName,
                p.NugetOrgPackageVersion);

            project.StateDetails = ProjectStateDetails.WaitingToBuild;
                
            project.AddUser(appUow.GetSimpleRepository<User>().GetByIdChecked(p.UserId));
            var projectNugetPackages = await ValidateAndCreateNugetPackages(p.NugetPackages, false);

            project.ProjectNugetPackages = projectNugetPackages;
            appUow.GetSimpleRepository<Project>().Create(project);

            systemMessages.Info(null, "Inserting project into Database", $"Inserting project into table in database");

            logger.LogTrace($"completed create project (after unitofwork.commit). Project: {project}");

            return project;
        }

        public async Task<int> CreateSingletonProject(CreateProjectParams p)
        {
            

            var project = await InsertIntoDb(p);

            systemMessages.Success(project,
                "Project has been created successfully",
                $"Project has been created successfully.\r\nProject: {project}\r\nDateTime: {DateTime.UtcNow}\r\n" +
                $"Nuget Packages:\r\n{string.Join("\r\n", project.ProjectNugetPackages.Select(t => t.ToStringIdentityIdVer()))}");

            return project.Id;
        }

        public async Task<List<NugetPackage>> ValidateAndCreateNugetPackages(IList<NugetPackageDto> nugetPackages, bool useLatestPackageIfVersionNull)
        {
            if (nugetPackages == null || nugetPackages.Count == 0) return new List<NugetPackage>();

            Validation.ThrowError(nugetPackages.Count > 25, "Current limit for nuget packages is 25");

            foreach (var p in nugetPackages)
            {
                Validation.NotStringIsNullOrWhiteSpace(p.IdentityId, "Null or empty nuget package IdentityId");

                Validation.ThrowError(
                    !useLatestPackageIfVersionNull && string.IsNullOrWhiteSpace(p.IdentityVersion),
                    $"Empty version for '{p.IdentityId}' package but version is required");
            }

            List<NugetPackage> projectPackages = new List<NugetPackage>();

            foreach (var npkg in nugetPackages)
            {
                PackageSearchMetadata pm = null;
                
                try
                {
                    if (useLatestPackageIfVersionNull && string.IsNullOrWhiteSpace(npkg.IdentityVersion))
                    {
                        pm = nugetRepositoryFacade.GetLatestPackage(npkg.IdentityId);
                    }
                    else
                    {
                        pm = nugetRepositoryFacade.GetPackageMetadata(npkg.IdentityId, npkg.IdentityVersion);
                    }
                }
                catch (Exception e)
                {
                    Validation.ThrowFieldErrorCore(nameof(nugetPackages), $"Failed to fetch nuget package: {npkg.IdentityId} {npkg.IdentityVersion}.");
                }

                var pp = new NugetPackage(
                    pm.Title,
                    pm.IdentityVersion,
                    pm.IdentityId,
                    pm.Published,
                    pm.ProjectUrl,
                    pm.PackageDetailsUrl,
                    pm.IsListed);

                projectPackages.Add(pp);
            }

            return projectPackages;
        }

        public async Task ValidateCreateProjectVersion(int projectVersioningId, string gitTagName, IList<NugetPackageDto> nugetPackages)
        {
            logger.LogTrace("starting validate create project version: projectVersioningId: {0}; gitTag: {1}; nugetPkgs: {2}",
                projectVersioningId, gitTagName, nugetPackages?.StringJoin(", ", t => $"{t.IdentityId} {t.IdentityVersion}"));

            var versioning = await projectVersioningRepo.GetByIdCheckedAsync(projectVersioningId);

            user.Forbidden(user.UserIdAuthorized != versioning.UserId, "Not owner of versioning");

            Validation.NotStringIsNullOrWhiteSpace(gitTagName);
            Validation.ThrowError(
                await projectRepo.GetByNameAsync(
                    Project.ProjectNameForVersioning(versioning.ProjectName, gitTagName)) != null,
                    "Cannot auto-generate project name for version. Project name is not unique");
            Validation.ThrowError(
                projectRepo.Query().Where(t => t.PVProjectVersioningId == projectVersioningId && t.PVGitTag == gitTagName).Any(),
                $"Project already exists for git tag ('{gitTagName}'). If You want to create again first delete existing project for this version."
                );

            await ValidateAndCreateNugetPackages(nugetPackages, false);

            if (!string.IsNullOrWhiteSpace(versioning.GitDocsRepoUrl))
            {
                throw new NotImplementedException();
                Validation.ThrowError(
                    !appManager.GitRepoExistsURL(versioning.GitDocsRepoUrl),
                    $"Git repository does not exists (in versioning): '{versioning.GitDocsRepoUrl}'");

                // using (var git = appManager.OpenGitRepo(versioning.GitDocsRepoUrl))
                // {
                //     Validation.ThrowError(!git.BranchExists(versioning.GitDocsBranchName), $"branch '{versioning.GitDocsBranchName}' does not exists");
                //     git.CheckoutBranch(versioning.GitDocsBranchName);
                //     Validation.FieldErrorP(!git.TagExists(gitTagName), gitTagName, "Tag does not exists in repository");
                // }
            }
        }

        public async Task<int> CreateVersionProject(int projectVersioningId, string gitTagName, IList<NugetPackageDto> nugetPackages)
        {
            gitTagName = gitTagName.Trim();

            logger.LogTrace("starting create project: projectVersioningId: {0}; gitTag: {1}; nugetPkgs: {2}",
                projectVersioningId, gitTagName, nugetPackages?.StringJoin(", ", t=> $"{t.IdentityId} {t.IdentityVersion}"));

            await ValidateCreateProjectVersion(projectVersioningId, gitTagName, nugetPackages);

            var versioningRepo = this.appUow.GetSimpleRepository<ProjectVersioning>();
            var versioning = versioningRepo.GetByIdChecked(projectVersioningId);
            string projectName = Project.ProjectNameForVersioning(versioning.ProjectName, gitTagName);

            string urlprefix = versioning.ProjectName;
            urlprefix = Project.NormalizeUrlPrefix(urlprefix);
            var existingProjWithUrl = projectRepo.Query().Any(t => t.ProjectType == ProjectType.Version && t.UrlPrefix == urlprefix);

            if (existingProjWithUrl)
            {
                Validation.ThrowError(true,
                   "Cannot generate project version, system found url prefix for project version." +
                   $"Url Prefix: {urlprefix}. But this prefix is used by project: {existingProjWithUrl}");
            }

            string tagCommitHash = null;

            throw new NotImplementedException();
            // using (var git = appManager.OpenGitRepo(versioning.GitDocsRepoUrl))
            // {
            //     tagCommitHash = git.GetTagCommitHash(gitTagName);
            // }

            var cpp =
                new CreateProjectParams(
                    projectName,
                    versioning.GitDocsRepoUrl,
                    null,
                    "Description: project version",
                    urlprefix,
                    nugetPackages,
                    user.UserIdAuthorized,
                    versioning.GitDocsRepoUrl,
                    versioning.GitDocsBranchName,
                    versioning.GitDocsRelativePath,
                    versioning.GitHomepageRelativePath,
                    tagCommitHash,
                    false,
                    ProjectType.Version,
                    null,
                    null);

            systemMessages.Trace(versioning, $"Versioning {versioning}: Starting Create Project",
                $"Starting to create versioning project with params: \r\n Git Tag: {gitTagName} \r\n Git Commit: {tagCommitHash} \r\n" + 
                $"Nuget Packages: \r\n {nugetPackages.StringJoin("\r\n", p => $"{p.IdentityId} {p.IdentityVersion}")}");

            var project = await InsertIntoDb(cpp);

            project.PVProjectVersioningId = versioning.Id;
            project.PVGitTag = gitTagName;

            return project.Id;
        }

        public async Task AutoupgradeProjectVersion(int projectVersioningId)
        {
            var versioning = appUow.GetSimpleRepository<ProjectVersioning>().GetByIdChecked(projectVersioningId);
            if (!versioning.Autoupgrage) return;

            systemMessages.Trace(versioning, $"{versioning}: Starting autoupgrade", "Starting to autoupgrade project versioning ");

            var versioningProjects = appUow.GetSimpleRepository<Project>()
                .Query().Where(t => t.PVProjectVersioningId == projectVersioningId)
                .ToList();

            string[] allTags = null;

            throw new NotImplementedException();
            // using (var git = appManager.OpenGitRepo(versioning.GitDocsRepoUrl))
            // {
            //     allTags = git.GetAllTags();
            // }
            
            if (allTags.Length == 0) return;

            allTags = allTags.OrderByDescending(t => t).ToArray();

            var latestTag = allTags.First();

            if (versioningProjects.Any(p => p.PVGitTag == latestTag))
            {
                systemMessages.Info(versioning, $"Project Versioning Autoupgrade: {versioning}, no upgrade  needed",
                    $"No need to upgrade Project Versioning: {versioning} because docs are generated for latest tag.\n" + 
                    $"Latest tag: {latestTag}\nAnd project is already online for this tag");

                return;
            }

            // generate max 3 versions from latest
            // (for safety purpose, to do not generate too many to hang a system, e.g. 100  tags or something like that)
            List<string> tagsToGenerate = new List<string>();
            
            for (int i = 0; i < 3 && i < allTags.Length && tagsToGenerate.Count < 3; i++)
            {
                if (versioningProjects.Any(p => p.PVGitTag == allTags[i])) tagsToGenerate.Add(allTags[i]);
                else break;
            }

            systemMessages.Trace(versioning, $"{versioning}: found tags to generate project",
                $"System will generate docs for following git tags:\n{tagsToGenerate.StringJoin("\n")}\n");

            for (int i = 0; i < tagsToGenerate.Count; i++)
            {
                systemMessages.Trace(versioning,
                    $"{versioning}: starting autoupgrade tag: {allTags[i]}",
                    $"Detected new tag to generate new project version\nnew tag: {allTags[i]}\nProject Versioning: {versioning}");

                await CreateVersionProject(projectVersioningId, allTags[i]);
            }

            return;
        }

        public async Task<int> CreateVersionProject(int projectVersioningId, string gitTagName)
        {
            // this method tries to get nuget packages matching git tag

            // somehow try  to find nuget package
            // version from git tag version, if not found 
            // this is impossible to find 'auto-find' nuget packages and error
            var versioning = await this.projectVersioningRepo.GetByIdCheckedAsync(projectVersioningId);
            var vNugetPackages = this.appUow.GetSimpleRepository<NugetPackage>()
                .Query()
                .Where(t => t.ProjectVersioningId == projectVersioningId)
                .ToList();

            Validation.NotStringIsNullOrWhiteSpace(gitTagName);
            List<string> possiblePkgVersions = new List<string>
            {
                System.Text.RegularExpressions.Regex.Match(gitTagName, "\\d+").Value,
                System.Text.RegularExpressions.Regex.Match(gitTagName, "\\d+\\.\\d+").Value,
                System.Text.RegularExpressions.Regex.Match(gitTagName, "\\d+\\.\\d+\\.\\d+").Value,
                System.Text.RegularExpressions.Regex.Match(gitTagName, "\\d+\\.\\d+\\.\\d+\\.\\d+").Value,
            }
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .OrderByDescending(t => t)
            .ToList();

            var packagesMatchingTagVer = new List<NugetPackageDto>();
            var altPackagesForMsg = new List<PackageSearchMetadata>();

            foreach (var package in vNugetPackages)
            {
                var packageMetadata = nugetRepositoryFacade.GetPackageMetadata(package.IdentityId)
                    .Where(t => !string.IsNullOrWhiteSpace(t.IdentityVersion))
                    .OrderByDescending(t => t.IdentityVersion)
                    .ToList();

                Validation.ThrowError(packageMetadata.Count == 0, $"no package metadata for nuget package: '{package.IdentityId}'");

                bool found = false;
                PackageSearchMetadata altPackage = null;
                PackageSearchMetadata exactPackage = null;

                foreach (var checkVer in possiblePkgVersions)
                {
                    foreach (var pm in packageMetadata)
                    {
                        
                        if (pm.IdentityVersion.StartsWith(checkVer))
                        {
                            found = true;
                            exactPackage = pm;
                            break;
                        }
                        else if (altPackage == null && string.CompareOrdinal(pm.IdentityVersion, checkVer) <= 0)
                        {
                            altPackage = pm;
                        }

                        if (found) break;
                    }

                    if (found) break;
                }

                altPackage = altPackage ?? packageMetadata.First();

                if (exactPackage != null)
                {
                    packagesMatchingTagVer.Add(new NugetPackageDto(exactPackage.IdentityId, exactPackage.IdentityVersion));
                }
                else
                {
                    altPackagesForMsg.Add(altPackage);
                    packagesMatchingTagVer.Add(new NugetPackageDto(altPackage.IdentityId, altPackage.IdentityVersion));
                }
            }

            if (altPackagesForMsg.Count > 0)
            {
                string warning = "System did not determine exact versions of nuget packages for git tag.\r\n" +
                    $"Selected following package (as alternative): \r\n{altPackagesForMsg.StringJoin("\r\n", t => $"{t.IdentityId} {t.IdentityVersion}")}\r\n" +
                    $"Git Tag Name: {gitTagName}\r\n" +
                    $"System tried following possible versions (parsed from git tag name):\r\n{possiblePkgVersions.StringJoin("\r\n")}\r\n";

                systemMessages.Warning(
                    versioning,
                    $"{versioning}: Alternative nuget package",
                    warning);
            }

            systemMessages.Trace(user.User,
                $"{versioning}: Nuget packages for git tag",
                $"Nuget packages for git tag. \r\n Git Tag: {gitTagName}\r\nPackages:\r\n" +
                $"{packagesMatchingTagVer.StringJoin("\r\n", t => $"{t.IdentityId} {t.IdentityVersion}")}");

            return await CreateVersionProject(projectVersioningId, gitTagName, packagesMatchingTagVer);
        }

        //public Task<int> CreateProjectVersion(int projectVersioningId, string gitTagName, IList<NugetPackageDto> nugetPackages)
        //{
        //    ValidateCreateProjectVersion(projectVersioningId, gitTagName, nugetPackages);
        //}
    }
}
