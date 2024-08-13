using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vinca.Api;
using Vinca.Exceptions;
using Vinca.Utils;

namespace DNDocs.Infrastructure.DomainServices
{
    public interface IGit : IDisposable
    {
        string RepoOSPath { get; }
        string NewestCommitHashForPath(string dirInRepoRelativePath);
        void InitInstance(string repoUrl);
        int GetCommitNoFromHEAD(string commit);
        void Pull();
        void FetchAll();
        void PruneRemote();
        bool BranchExists(string branchName);
        void CheckoutBranch(string branchName);
        void CheckoutCommit(string commit);
        bool RepositoryExistsURL(string url);
        bool TagExists(string gitTagName);
        string[] GetAllTags();
        string GetTagCommitHash(string gitTagName);
    }

    public class GitOptions
    {
        public string OSPathGitExe { get; set; }
    }

    // TODO: git ls-remote <repo-url> also shows tags
    // use it instead of local remote tag (in get tags e.g.);
    internal class Git : IGit, IDisposable
    {
        private IOSApi osapi;
        private ILogger<Git> logger;
        private string gitExeFilePath;
        private string rootDirOsPath; // folder contains repo folder
        private string repoUrl;
        private IServiceProvider serviceProvider;
        static ConcurrentDictionary<string, object> _lockedRepos = new ConcurrentDictionary<string, object>();

        public Git(
            IOSApi osapi,
            IOptions<GitOptions> options,
            IServiceProvider serviceProvider,
            ILogger<Git> logger)
        {
            this.osapi = osapi;
            this.logger = logger;
            this.gitExeFilePath = options.Value.OSPathGitExe;
            this.serviceProvider = serviceProvider;
        }

        public void InitInstance(string gitRepoUrl)
        {
            logger.LogTrace("initializing: {0}", gitRepoUrl);

            VValidate.AppEx(this.repoOSPath != null, $"Invalid operation: This repo is already initialized, cannot initialize again. Repo: {repoOSPath}");
            VValidate.AppEx(string.IsNullOrWhiteSpace(gitRepoUrl), "repoUrl is null or empty");

            this.repoUrl = gitRepoUrl;

            // what if multiple requests want same repo at same time? wait max 5s if not possible throw exception (e.g. builing project or something)
            bool currentThreadLockedOk = false;
            for (int i = 0; i < 100 && !(currentThreadLockedOk = _lockedRepos.TryAdd(this.repoUrl, null)); i++)
                Thread.Sleep(50);

            if (!currentThreadLockedOk)
            {
                // todo: maybe check how long is locked? e.g. if longer than 5 minutes then unlock/delete entire repo (maybe this is internal error)?
                logger.LogError("Repo already locked, throwing exception:\nRepoUrl: {0}\nallLockerrepos: {1}", gitRepoUrl, _lockedRepos.StringJoin("\n", t => $"{t.Key}"));

                VValidate.AppEx($"repository already locked: '{repoUrl}'");
            }

            // in db save in lowercase (for comparison reasons)
            var repoUrlNormalized = this.repoUrl.Trim().ToLower();
            var gitRepoStoreUuid = Guid.Empty;

            throw new NotImplementedException("implement below commented");
            //using (var scope = this.serviceProvider.CreateScope())
            //{
            //    var scopeuow = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>();
            //    var scopeRepo = scopeuow.GetSimpleRepository<GitRepoStore>();
            //    var existing = scopeRepo.Query().Where(t => t.GitRepoUrl == repoUrlNormalized).FirstOrDefault();

            //    if (existing == null)
            //    {
            //        var newGitRepoStore = new GitRepoStore(repoUrlNormalized);
            //        scopeuow.GetSimpleRepository<GitRepoStore>().Create(newGitRepoStore);

            //        gitRepoStoreUuid = newGitRepoStore.UUID;
            //    }
            //    else
            //    {
            //        existing.LastAccessOn = DateTime.UtcNow;
            //        gitRepoStoreUuid = existing.UUID;
            //    }
            //}

            throw new Exception("implement below");
            // string repoOsFolderPath = appManager.GetOSPathGitRepoStoreRepo(gitRepoStoreUuid);
            string repoOsFolderPath = "TODO IMPLEMNET";

            rootDirOsPath = repoOsFolderPath;
            bool clonedNew = false;

            if (!Directory.Exists(repoOsFolderPath))
            {
                logger.LogTrace("creating directory '{0}'", repoOsFolderPath);

                Directory.CreateDirectory(repoOsFolderPath);
                osapi.ProcessStart(gitExeFilePath, $"clone {repoUrl}", 30, out _, out var stdout, out _, workingDirectory: repoOsFolderPath);
                clonedNew = true;
            }

            var allDirs = Directory.GetDirectories(rootDirOsPath);

            VValidate.AppEx(allDirs.Length != 1,
                "Git root directory must contain single directory with cloned repo, " +
                $"but there is no single directory in current root.\r\nroot repo dir: {rootDirOsPath}\r\nfound directories:\r\n{allDirs.StringJoin(",\r\n")}");

            repoOSPath = allDirs.First();
            if (clonedNew) Pull();
        }

        private string repoOSPath = null;
        public string RepoOSPath { get { return repoOSPath ?? throw new VAppException("Cannot access RepoOSPath because instance not initialized. Call 'InitInstance()' method before using methods"); } }

        public void PruneRemote()
        {
            GitCmd("remote prune origin");
        }

        public void FetchAll()
        {
            GitCmd("fetch --all");
        }

        public bool BranchExists(string branchName)
        {
            string branches;
            GitCmd("branch -a", out branches);

            return branches.Contains($"origin/{branchName}");
        }

        public void CheckoutBranch(string branchName)
        {
            string curBranch;
            GitCmd("rev-parse --abbrev-ref HEAD", out curBranch);
            curBranch = curBranch.Trim();

            if (curBranch == branchName) return;

            // todo - check if branch already created and 
            // just checkout (without 'origin/branchname')
            // e.g. checkout {branchName}

            GitCmd("branch", out var stdout);
            bool branchCreated = stdout.Split('\n').Select(t => t.Trim()).Any(t => t == branchName);

            if (!branchCreated)
            {
                GitCmd($"checkout -b {branchName} origin/{branchName}");
            }
            else if (curBranch != branchName)
            {
                GitCmd($"checkout {branchName}");
            }
        }

        public string NewestCommitHashForPath(string folderPathInRepoRelative)
        {
            // git log --pretty=tformat:"%H" -n1 .
            string stdout;
            GitCmd($"log --pretty=tformat:\"%H\" -n1 {folderPathInRepoRelative}", out stdout);

            string commit = stdout?.Trim();

            return commit;
        }

        public void Pull()
        {
            GitCmd("pull");
        }

        public string[] GetAllTags()
        {
            GitCmd("tag", out var stdout);
            var endline = stdout.Contains("\r\n") ? "\r\n" : "\n";

            string[] tags = stdout?.Split(endline) ?? new string[0];
            tags = tags.Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
            tags = tags.OrderByDescending(t => t).ToArray();

            return tags;
        }

        public bool RepositoryExistsURL(string giturl)
        {
            bool exists = false;
            try
            {
                osapi.ProcessStart(gitExeFilePath, $"ls-remote {giturl}", 30, out _, out var stdout, out _);
                exists = true;
            }
            catch (Exception e)
            {
                exists = false;
            }

            return exists;
        }

        public string GetTagCommitHash(string gitTagName)
        {
            GitCmd($"rev-list -n 1 {gitTagName}", out var stdout);

            return stdout?.Trim();
        }

        private void GitCmd(string cmd)
        {
            GitCmd(cmd, out _);
        }

        private void GitCmd(string cmd, out string stdout)
        {
            osapi.ProcessStart(gitExeFilePath, cmd, 30, out _, out stdout, out _, workingDirectory: this.RepoOSPath);
        }

        public bool TagExists(string gitTagName) => GetAllTags().Any(t => t == gitTagName);

        ~Git() { Dispose(); }

        public void Dispose()
        {
            if (repoUrl == null) return;

            try
            {
                _lockedRepos.TryRemove(repoUrl, out _);
            }
            catch
            {

            }
        }

        public void CheckoutCommit(string commit)
        {
            VValidate.AppEx(string.IsNullOrWhiteSpace(commit), "commit empty");
            GitCmd($"checkout {commit}");
        }

        public int GetCommitNoFromHEAD(string commit)
        {
            VValidate.AppEx(string.IsNullOrWhiteSpace(commit), "commit empty");
            GitCmd($"rev-list  {commit}.. --count", out var stdout);

            return int.Parse(stdout.Trim());
        }
    }
}
