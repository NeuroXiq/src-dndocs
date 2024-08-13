using Microsoft.EntityFrameworkCore;
using DNDocs.Application.Queries.ProjectManage;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Repository;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Infrastructure.Utils;
using DNDocs.Api.DTO.ProjectManage;
using DNDocs.Api.DTO.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.QueryHandlers.ProjectManage
{
    internal class GetProjectsVersioningInfoHandler : QueryHandlerA<GetProjectsVersioningInfoQuery, TableDataDto<ProjectVersioningInfoDto>>
    {
        private ICurrentUser user;
        private ICache cache;
        private IAppUnitOfWork uow;
        private IAppManager appManager;
        private IRepository<ProjectVersioning> versioningRepo;
        private IRepository<Project> projectRepo;

        public GetProjectsVersioningInfoHandler(ICache cache, ICurrentUser user, IAppUnitOfWork uow, IAppManager appManager)
        {
            this.user = user;
            this.cache = cache;
            this.uow = uow;
            this.appManager = appManager;
            this.versioningRepo = uow.GetSimpleRepository<ProjectVersioning>();
            this.projectRepo = uow.GetSimpleRepository<Project>();
        }

        protected override async Task<TableDataDto<ProjectVersioningInfoDto>> Handle(GetProjectsVersioningInfoQuery query)
        {
            var versioning = await versioningRepo.GetByIdCheckedAsync(query.ProjectVersioningId);
            string[] tags = null;

            if (!cache.TryGetJKM<string[]>(this, query.ProjectVersioningId.ToString(), out tags))
            {
                throw new NotImplementedException();
                // using (var git = appManager.OpenGitRepo(versioning.GitDocsRepoUrl))
                // {
                //     git.Pull();
                //     tags = git.GetAllTags();
                // }

                cache.AddJKM(this, query.ProjectVersioningId.ToString(), tags, TimeSpan.FromMinutes(15));
            }

            tags = tags.OrderByDescending(t => t).ToArray();
            var tagsPage = tags.Skip(query.PageNo * 10).Take(10).ToArray();
            var projectsForTags = await projectRepo.Query().Where(t => tags.Contains(t.PVGitTag) && t.PVProjectVersioningId == versioning.Id).ToArrayAsync();

            var data = tagsPage.Select(t =>
            {
                var project = projectsForTags.FirstOrDefault(p => p.PVGitTag == t);
                return new ProjectVersioningInfoDto(t, project?.Id, Mapper.Map(project?.ProjectNugetPackages));
            });

            return new TableDataDto<ProjectVersioningInfoDto>(tags.Length, query.PageNo, 10, data);
        }
    }
}
