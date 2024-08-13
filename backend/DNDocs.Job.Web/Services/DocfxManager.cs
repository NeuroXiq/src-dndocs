
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using DNDocs.Resources;
using Vinca.Exceptions;
using Vinca.Api;
using DNDocs.Job.Web.Shared;
using DNDocs.Job.Web.ValueTypes;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using DNDocs.Job.Api.Management;

namespace DNDocs.Job.Web.Services
{
    public interface IDocfxManager
    {
        string OSPathSiteDirectory { get; }
        string OSPathBinDir { get; }
        string OSPathArticlesDir { get; }
        string OSPathIndexHtml { get; }
        string RootDirectory { get; }
        public string OSPathDocfxJson { get; }

        void Init(string directory, BuildProjectModel p);

        // void SetHomepageContent(string content);
        void CleanAfterBuild();
        void AutogenerateArticlesTOC();
        void SetDocfxJson(string appFooterHtml, string appTitle, string templateName);
        void SetHomepageContent(string content);
        void SetApiHomepageContent(string content);
        void SetArticlesHomepageContent(string content);
    }

    public class DocfxManager : IDocfxManager
    {
        const string _site = "_site";
        private ILogger<DocfxManager> logger;
        private IOSApi osapi;
        private DJobSettings dsettings;

        private string pathSiteDirectory;
        private string ospathApiDir;

        private string _rootDirectory = null;
        private string rootDirectory
        {
            get
            {
                return _rootDirectory ?? throw new InvalidOperationException("Root dir not set. Call 'Open' method");
            }
        }
        private DocfxJson docfxJson = null;

        public string OSPathDocfxJson { get; private set; }
        public string OSPathSiteDirectory { get { return pathSiteDirectory; } }
        public string OSPathApiDirectory { get { return ospathApiDir ?? throw new InvalidOperationException("internal: not initialized apidir"); } }
        public string OSPathArticlesDir { get { return Path.Combine(rootDirectory, "articles"); } }
        public string RootDirectory { get { return this.rootDirectory; } }
        public string OSPathBinDir => Path.Combine(rootDirectory, "bin");
        public string OSPathIndexHtml => Path.Combine(rootDirectory, "index.md");

        public DocfxManager(
            IOSApi osapi,
            ILogger<DocfxManager> logger,
            IOptions<DJobSettings> dsettings)
        {
            this.logger = logger;
            this.osapi = osapi;
            this.dsettings = dsettings.Value;
        }

        private void SetRootDirectory(string directory)
        {
            VValidate.Throw(!Directory.Exists(directory), $"Cannot open docfx project because directory does not exist. Directory: '{directory ?? ""}'");

            this._rootDirectory = directory;
            this.pathSiteDirectory = Path.Combine(rootDirectory, "_site");
            this.ospathApiDir = Path.Combine(rootDirectory, "api");
            this.OSPathDocfxJson = Path.Combine(rootDirectory, "docfx.json");
        }

        private void DeleteCurrentFolder(string directory)
        {
            if (!directory.EndsWith("docfx_project"))
                throw new InvalidOperationException("not ends with 'docfx_project' throws for safety ");

            Directory.Delete(directory, true);
        }

        public void Init(string directory, BuildProjectModel project)
        {
            directory = Path.Combine(directory, "docfx_project");

            if (Directory.Exists(directory))
            {
                DeleteCurrentFolder(directory);
            }

            Directory.CreateDirectory(directory);
            SetRootDirectory(directory);

            // copy-paste all files that command  'docfx init' does
            // all those files were copy&paster to Resources project
            // to be sure that we have everything the same between migrating
            // to newer version of docfx (e.g. maybe next version 'docfx init' will create other structure?)
            var substringLen = AppResources.DocfxInitDir.Length;
            var allDirs = Directory.GetDirectories(AppResources.DocfxInitDir, "*", SearchOption.AllDirectories)
                .Select(t => t.Substring(substringLen))
                .Order()
                .ToList();

            var allFiles = Directory.GetFiles(AppResources.DocfxInitDir, "*", SearchOption.AllDirectories)
                .Select(t => t.Substring(substringLen + 1))
                .ToList();

            foreach (var dir in allDirs) Directory.CreateDirectory(OSFullPath(dir));
            foreach (var file in allFiles)
            {
                File.Copy(Path.Combine(AppResources.DocfxInitDir, file), OSFullPath("/" + file));
            }

            Directory.CreateDirectory(OSFullPath("/bin"));

            docfxJson = DocfxJson.FromJson(File.ReadAllText(OSPathDocfxJson));

            // default values
            SetDocfxJson("default-footer", "default-title", null);
        }
        
        public void SetArticlesHomepageContent(string content) => ContentUpdateFile("/articles/intro.md", content);
        
        public void SetHomepageContent(string content) => ContentUpdateFile("/index.md", content);
        
        public void SetApiHomepageContent(string content) => ContentUpdateFile("/api/index.md", content);

        public void SetDocfxJson(string appFooterHtml, string appTitle, string templateName)
        {
            var ngm = new DocfxJson.NBuild.NGlobalMetadata();

            if (!string.IsNullOrWhiteSpace(templateName))
            {
                var templatesDir = DNDocs.Resources.AppResources.DocfxTemplatesDirOSPath;
                VValidate.Throw(string.IsNullOrWhiteSpace(templateName), nameof(templateName));
                var osPathTemplate = Path.Combine(templatesDir, templateName);

                VValidate.Throw(!Directory.Exists(osPathTemplate), $"Template '{templateName}' does not exists in templates dir: {templatesDir}");

                docfxJson.Build.Template.Add(osPathTemplate);
            }

            ngm._appFooter = appFooterHtml;
            ngm._appTitle = appTitle;
            ngm._enableSearch = true;
            ngm.pdf = false;

            docfxJson.Build.GlobalMetadata = ngm;

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(this.docfxJson, serializeOptions);
            File.WriteAllText(this.OSPathDocfxJson, json);
        }

        // during investigation other bug code from here was moved to ConsoleTools project,
        // but moving this didn;t solve that bug (other solution found). But I leave this code in separate
        // project anyway. (Code from ConsoleTools can be copied here directly and should work ok)
        //
        // But not sure:
        // 1. All ..DocAsCode... is static classes/methods - is this thread safe??
        // 2. additionally it creates its own 'templates' folder that is not needed
        // 3.Lots of console logs (not sure if the are mixed if multiple thread are running, is this thread-safe? to use this static class)
        // so for now leaving it in separate process seems to be safer

        //public void BinSetFiles(IList<BlobData> files)
        //{
        //    var bpath = OSFullPath($"/bin");
        //    var dirinfo = new DirectoryInfo(bpath);
        //    foreach (var f in dirinfo.GetFiles()) f.Delete();

        //    foreach (var file in files)
        //    {
        //        Validation.ThrowError(
        //            string.IsNullOrWhiteSpace(file.OriginalName),
        //            "DfxManager: File name null or white space");

        //        Validation.ThrowError(
        //            !file.OriginalName.All(c =>
        //                (c >= 'A' && c <= 'Z') ||
        //                (c >= 'a' && c <= 'z') ||
        //                (c >= '0' && c <= '9') ||
        //                (c == '.') ||
        //                (c == '-')),
        //            "DfxManager: cannot set bin files, invalid char in filename");

        //        Validation.ThrowError(
        //            (!file.OriginalName.EndsWith(".dll") && !file.OriginalName.EndsWith(".xml")),
        //            "DfxManager: Invalid files (not xml and dll).");

        //        Validation.ThrowError(
        //            file.OriginalName.Contains(".."),
        //            "DfxManager: file name double dots '..'");

        //        var binpath = OSFullPath($"/bin/{file.OriginalName}");

        //        File.WriteAllBytes(binpath, file.ByteData);
        //    }
        //}


        public void ContentUpdateFile(string vPath, string newContent)
        {
            ValidateVPathFile(vPath, nameof(vPath), mustExists: true);

            VValidate.ThrowError(
                    !vPath.EndsWith(".md") &&
                    !vPath.EndsWith("toc.yml"),
                    "This file is not .md and toc.yml file");

            VValidate.ThrowError(
                    newContent != null && newContent.Length > (5000000),
                    "Max file size 5000000 bytes");

            var filename = vPath.Split('/').Last();
            var parentVPath = vPath.Substring(0, vPath.Length - filename.Length);

            var ospath = OSFullPath(vPath);

            File.WriteAllText(ospath, newContent);
        }

        public void CleanAfterBuild()
        {
            DeleteCurrentFolder(this.RootDirectory);

            // preserve this files, they are needed
            // var gitignorePath = OSFullPath("/api/.gitignore");
            // var indexPath = OSFullPath("/api/index.md");
            // var tocPath = OSFullPath("/api/toc.yml");
            // 
            // var gitignore = File.ReadAllBytes(gitignorePath);
            // var index = File.ReadAllBytes(indexPath);
            // var toc = File.ReadAllBytes(tocPath);
            // 
            // Directory.Delete(OSPathSiteDirectory, true);
            // Directory.Delete(OSPathApiDirectory, true);
            // 
            // Directory.CreateDirectory(OSPathApiDirectory);
            // File.WriteAllBytes(gitignorePath, gitignore);
            // File.WriteAllBytes(indexPath, index);
            // File.WriteAllBytes(tocPath, toc);
        }

        private string OSFullPath(string vpath)
        {
            vpath = vpath.TrimStart('/', '\\');
            vpath = vpath.Replace('/', Path.DirectorySeparatorChar);

            var result = Path.Combine(this.rootDirectory, vpath);

            VValidate.Throw(!result.StartsWith(this.rootDirectory), "Safety: something is bad with directory");

            return result;
        }

        void ValidateVPathShared(
            string vpath,
            string nameofArg)
        {
            bool isvalid = !string.IsNullOrWhiteSpace(vpath);
            isvalid &= vpath.StartsWith("/articles") || vpath == "/index.md" || vpath == "/api/index.md";


            VValidate.ThrowError(!isvalid, $"Invalid path '{vpath}'");
            VValidate.ThrowError(
                !vpath.All(d => ((d >= 'a' && d <= 'z') || (d >= '0' && d <= '9') || d == '/' || d == '.' || d == '-')),
                $"Invalid character in path. '{vpath}'");

            VValidate.ThrowError(vpath.Length > 80, $"Path is too big (max 80 chars) '{vpath}'");
        }

        void ValidateVPathFile(string vpath, string argName, bool mustExists = false)
        {
            ValidateVPathShared(vpath, argName);

            VValidate.ThrowError(string.IsNullOrWhiteSpace(vpath), "null or empty path");

            VValidate.ThrowError(
                !vpath.EndsWith(".md") &&
                !vpath.EndsWith(".yml"),
                $"Invalid extension. file: {vpath}");

            VValidate.ThrowError(vpath.Count(c => c == '.') > 1, $"InvalidName contains '.'. ''{vpath}''");

            var split = vpath.Split('.');
            var filename = split[1].Split('/').Last();
            VValidate.ThrowError(filename.Length < 1, $"Empty file name {vpath}");

            var osfull = OSFullPath(vpath);

            if (mustExists)
            {
                VValidate.ThrowError(!File.Exists(osfull), $"file not exists '{vpath}'");
            }
        }

        public void AutogenerateArticlesTOC()
        {
            var files = Directory.GetFiles(OSPathArticlesDir, "*.*", SearchOption.TopDirectoryOnly);

            if (!files.Any(f => Path.GetFileName(f).ToLower() == "toc.yml"))
            {
                File.Create(Path.Combine(OSPathArticlesDir, "toc.yml")).Dispose();
            }

            File.WriteAllText(Path.Combine(OSPathArticlesDir, "toc.yml"), "");

            var toc = GenerateTOC(OSPathArticlesDir);

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(toc);

            File.WriteAllText(Path.Combine(OSPathArticlesDir, "toc.yml"), yaml);
        }

        public IList<TOCItem> GenerateTOC(string parentFolder)
        {
            var currentFiles = Directory
                .GetFiles(parentFolder, "*.md", SearchOption.TopDirectoryOnly).OrderBy(t => t.Length)
                .Select(t => t.Substring(OSPathArticlesDir.Length + 1))
                .ToList();

            var currentDirs = Directory.GetDirectories(parentFolder, "*", SearchOption.TopDirectoryOnly);

            List<TOCItem> result = new List<TOCItem>();

            foreach (var f in currentFiles)
            {
                var fileRow = new TOCItem()
                {
                    Name = TOCName(Path.GetFileName(f)),
                    Href = f.Replace('\\', '/')
                };

                result.Add(fileRow);
            }

            foreach (var dir in currentDirs)
            {
                var di = new DirectoryInfo(dir);
                var dirRow = new TOCItem()
                {
                    Name = TOCName(di.Name),
                    Items = GenerateTOC(dir)
                };

                result.Add(dirRow);
            }

            result = result.OrderBy(t => t.Name).ToList();

            return result;
        }

        private string TOCName(string filename)
        {
            filename = filename.EndsWith(".md") ? filename.Substring(0, filename.Length - 3) : filename;

            if (filename.Length < 2) return filename;

            filename = char.ToUpper(filename[0]) + filename.Substring(1);
            var tosplit = new char[] { '-', '.', ' ' };
            filename = string.Join(" ", filename.Split(tosplit));

            return filename;
        }
    }

    public class DocfxJson
    {
        public class NMetadata
        {
            public class NSrc
            {

                public List<string> Files { get; set; }
            }

            public List<NSrc> Src { get; set; }

            public string Dest { get; set; }

            public bool IncludePrivateMembers { get; set; }

            public bool DisableGitFeatures { get; set; }

            public bool DisableDefaultFilter { get; set; }

            public bool NoRestore { get; set; }

            public string NamespaceLayout { get; set; }

            public string MemberLayout { get; set; }

            public bool AllowCompilationErrors { get; set; }
        }

        public class NBuild
        {
            public class NGlobalMetadata
            {

                public string _appTitle { get; set; }

                public string _appFooter { get; set; }

                public string _appLogoPath { get; set; }

                public string _appFaviconPath { get; set; }

                public bool _enableSearch { get; set; }

                public string _enableNewTab { get; set; }

                public string _disableNavbar { get; set; }

                public string _disableBreadcrumb { get; set; }

                public string _disableToc { get; set; }

                public string _disableAffix { get; set; }

                public string _disableContribution { get; set; }

                public string _gitContribute { get; set; }

                public string _gitUrlPattern { get; set; }

                public string _noindex { get; set; }

                public bool pdf { get; set; }
            }

            public class NContent
            {
                public List<string> Files { get; set; }

                public NContent() { }

                public NContent(List<string> files)
                {
                    Files = files;
                }
            }

            public class NResource
            {

                public List<string> Files { get; set; }
            }

            public List<NContent> Content { get; set; }

            public List<NResource> Resource { get; set; }

            public string Dest { get; set; }

            public string Output { get; set; }

            public List<string> GlobalMetadataFiles { get; set; }

            public List<string> FileMetadataFiles { get; set; }

            public List<string> Template { get; set; }

            public List<string> PostProcessors { get; set; }

            public bool KeepFileLink { get; set; }

            public bool DisableGitFeatures { get; set; }

            public NGlobalMetadata GlobalMetadata { get; set; }
        }

        public NBuild Build { get; set; }
        public List<NMetadata> Metadata { get; set; }

        public static DocfxJson FromJson(string json)
        {
            JsonSerializerOptions opt = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var x = JsonSerializer.Deserialize<DocfxJson>(json, opt);

            return x;
        }
    }

    public class TOCItem
    {
        public string Name { get; set; }
        public string Href { get; set; }
        public string Homepage { get; set; }
        public bool Expanded { get; set; }
        public IList<TOCItem> Items { get; set; }
    }
}
