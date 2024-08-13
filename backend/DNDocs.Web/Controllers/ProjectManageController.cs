
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DNDocs.Application.Commands;
using DNDocs.Application.Commands.Projects;
using DNDocs.Application.Queries;
using DNDocs.Application.Queries.ProjectManage;
using DNDocs.Application.Shared;
using DNDocs.Web.Application.RateLimit;
using DNDocs.Web.Models.MyAccount;
using DNDocs.Web.Models.Project;
using DNDocs.Api.DTO;
using DNDocs.Api.DTO.ProjectManage;
using DNDocs.Domain.ValueTypes;

namespace DNDocs.Web.Controllers
{
    [Authorize]
    public class ProjectManageController : ApiControllerBase
    {
        private readonly IMapper mapper;
        private readonly IQueryDispatcher qd;
        private readonly ICommandDispatcher cd;


        public ProjectManageController(
            ICommandDispatcher cd,
            IQueryDispatcher qd,
            IMapper mapper)
        {
            this.mapper = mapper;
            this.qd = qd;
            this.cd = cd;
        }

        [HttpGet]
        public async Task<IActionResult> GetVersioningGitTags(int projectVersioningId)
        {
            return await ApiResult2(qd.DispatchAsync(new GetProjectVersioningGitTagsQuery() { ProjectVersioningId = projectVersioningId }));
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectsVersioningInfo(int projectVersioningId, int pageNo)
        {
            return await ApiResult2(qd.DispatchAsync(new GetProjectsVersioningInfoQuery() { PageNo = pageNo, ProjectVersioningId = projectVersioningId }));
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectVersioningById(int id)
        {
            return await ApiResult2(qd.DispatchAsync(new GetProjectVersioningQuery() { Id = id }));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjectVersioning()
        {
            var a = await qd.DispatchAsync(new GetAllProjectVersioningQuery());

            return await ApiResult2(Task.FromResult(a));
        }

        //
        // POST
        //

        [HttpDelete]
        [RateLimit(RLP.Project)]
        public async Task<IActionResult> DeleteProjectVersioning([FromBody] int projectVersioningId)
        {
            return await ApiResult2(cd.DispatchAsync(new DeleteProjectVersioningCommand { ProjectVersioningId = projectVersioningId }));
        }

        [HttpPost]
        [RateLimit(RLP.Project)]
        public async Task<IActionResult> CreateProjectVersionByGitTag(CreateProjectVersionByGitTagModel model)
        {
            return await ApiResult2(cd.DispatchAsync(new CreateProjectVersionByGitTagCommand()
            {
                ProjectVersioningId = model.ProjectVersioningId,
                GitTagName = model.GitTagName
            }));
        }

        [HttpPost]
        [RateLimit(RLP.Project)]
        public async Task<IActionResult> CreateProjectVersion( CreateProjectVersionModel model)
        {
            var nugetPackages = model.NugetPackages?
                .Select(t => new Domain.ValueTypes.NugetPackageDto(t.IdentityId, t.IdentityVersion))
                .ToList()
                ?? new List<Domain.ValueTypes.NugetPackageDto>();

            return await ApiResult2(cd.DispatchAsync(new CreateProjectVersionCommand {
                GitTagName = model.GitTagName,
                NugetPackages = nugetPackages,
                ProjectVersioningId = model.ProjectVersioningId }));
        }

        [HttpPost]
        [RateLimit(RLP.Project)]
        public async Task<IActionResult> CreateProjectVersioning(CreateProjectVersioningModel model)
        {
            return await ApiResult2(cd.DispatchAsync(new CreateProjectVersioningCommand(
                model.ProjectName,
                model.ProjectWebsiteUrl,
                model.UrlPrefix,
                model.GitDocsRepoUrl,
                model.GitDocsBranchName,
                model.GitDocsRelativePath,
                model.GitHomepageRelativePath,
                model.NugetPackages,
                model.Autoupgrage
                )));
        }

        [HttpPost]
        [RateLimit(RLP.Project)]
        public async Task<IActionResult> RequestProject([FromForm] RequestProjectModel model)
        {
            CreateSingletonProjectCommand cmd = new CreateSingletonProjectCommand()
            {
                ProjectName = model.ProjectName,
                GithubUrl = model.GithubUrl,
                DocfxTemplate = model.DocfxTemplate,
                Description = model.Description,
                UrlPrefix = model.UrlPrefix,
                NugetPackages = model.NugetPackages?.Select(p => new Domain.ValueTypes.NugetPackageDto(p, null)).ToList(),
                GitMdBranchName = model.GitMdBranchName,
                GitMdRepoUrl = model.GitMdRepoUrl,
                GitMdRelativePathDocs = model.GitMdRelativePathDocs,
                GitMdRelativePathReadme = model.GitMdRelativePathReadme,
                PSAutoRebuild = model.PSAutoRebuild,
            };

            var a = await cd.DispatchAsync(cmd);
            return await ApiResult2(Task.FromResult(a));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProject([FromBody] int projectid)
        {
            return await ApiResult2(cd.DispatchAsync(new DeleteProjectCommand(projectid)));
        }

        [HttpPost]
        public async Task<IActionResult> GetProjectById([FromBody] int projectid)
        {
            return await ApiResult2(qd.DispatchAsync(new GetProjectByIdQuery(projectid)));
        }

        [HttpPost]
        [RateLimit(RLP.Project)]
        public async Task<IActionResult> RequestAutoupgrade([FromBody] int projectid)
        {
            return await ApiResult2(cd.DispatchAsync(new RequestAutoupgradeSingletonProjectCommand { ProjectId = projectid }));
        }
    }
}

/*
 * TODO: need to support this feature?
 * 
 #region Docfx
[HttpGet]
        public IActionResult DocfxGetFileContent(int projectid, string vpath)
        {
            return ApiResult2(qd.DispatchSync();
        }

        [HttpGet]
        public IActionResult GetDocfxContentItems(int projectid)
        {
            var result = this.projectManageService.GetDocfxContentItems(projectid);

            return ApiResult2(result);
        }

        public class DocfxCreateDirForm
        {
            public int ProjectId { get; set; }
            public string Directory { get; set; }
        }

        [HttpPost]
        public IActionResult DocfxCreateDirectory([FromBody] DocfxCreateDirForm form)
        {
            return ApiResult2(projectManageService.DocfxCreateDirectory(form.ProjectId, form.Directory));
        }

        public class RemoveDirModel
        {
            public int ProjectId { get; set; }
            public string DirectoryPath { get; set; }
        }

        [HttpPost]
        public IActionResult DocfxRemoveDirectory([FromBody] RemoveDirModel model)
        {
            return ApiResult2(this.projectManageService.DocfxRemoveDirectory(model.ProjectId, model.DirectoryPath));
        }

        public class DocfxMoveDirectoryModel
        {
            public int ProjectId { get; set; }
            public string VPathSrc { get; set; }
            public string VPathDest { get; set; }
        }

        [HttpPost]
        public IActionResult DocfxMoveDirectory([FromBody] DocfxMoveDirectoryModel model)
        {
            return ApiResult2(projectManageService.DocfxMoveDirectory(model.ProjectId, model.VPathSrc, model.VPathDest));
        }

        public class DocfxAddFileModel
        {
            public int ProjectId { get; set; }
            public string ParentDirectory { get; set; }
            public string FileName { get; set; }
            public string Extension { get; set; }
        }

        [HttpPost]
        public IActionResult DocfxCreateFile([FromBody] DocfxAddFileModel model)
        {
            return ApiResult2(projectManageService.DocfxCreateFile(model.ProjectId, model.ParentDirectory, model.FileName, model.Extension));
        }

        public class DocfxMoveFileModel
        {
            public int ProjectId { get; set; }
            public string VPathSrc { get; set; }
            public string VPathDest { get; set; }
        }

        [HttpPost]
        public IActionResult DocfxMoveFile([FromBody] DocfxMoveFileModel model)
        {
            return ApiResult2(this.projectManageService.DocfxMoveFile(model.ProjectId, model.VPathSrc, model.VPathDest));
        }


        public class RemoveFileModel
        {
            public int ProjectId { get; set; }
            public string FilePath { get; set; }
        }

        [HttpPost]
        public IActionResult DocfxRemoveFile([FromBody] RemoveFileModel model)
        {
            return ApiResult2(this.projectManageService.DocfxRemoveFile(model.ProjectId, model.FilePath));
        }

        public class DocfxUpdateFileModel
        {
            public int ProjectId { get; set; }
            public string VPath { get; set; }
            public string NewContent { get; set; }
        }

        [HttpPost]
        public IActionResult DocfxUpdateFile([FromBody] DocfxUpdateFileModel model)
        {
            return ApiResult2(this.projectManageService.DocfxUpdateFile(model.ProjectId, model.VPath, model.NewContent));
        }

        public class DocfxUploadFileModel
        {
            public int ProjectId { get; set; }
            public IFormFile File { get; set; }
            public string VPath { get; set; }
        }

        [HttpPost]
        public IActionResult DocfxUploadFile(DocfxUploadFileModel model)
        {
            return ApiResult2(projectManageService.DocfxUploadFile(model.ProjectId, model.VPath, mapper.Map<FormFileDto>(model.File)));
        }

        [HttpPost]
        public IActionResult RequestDocfxBuildProject([FromBody] int projectid)
        {
            return ApiResult2(cd.Dispatch(new RequestDocfxBuildProjectCommand { ProjectId = projectid }));
        }

        #endregion
 
 */