using DNDocs.Application.Queries.ProjectManage;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.QueryHandlers.ProjectManage
{
    class GetProjectVersioningGtiTagsHandler : QueryHandlerA<GetProjectVersioningGitTagsQuery, string[]>
    {
        private ICurrentUser user;
        private ICache cache;
        private IAppManager appManager;
        private IAppUnitOfWork uow;

        public GetProjectVersioningGtiTagsHandler(
            IAppManager appManager,
            IAppUnitOfWork uow,
            ICurrentUser user,
            ICache cache)
        {
            this.user = user;
            this.cache = cache;
            this.appManager = appManager;
            this.uow = uow;
        }

        protected override async Task<string[]> Handle(GetProjectVersioningGitTagsQuery query)
        {
            var versioning = uow.GetSimpleRepository<ProjectVersioning>().GetByIdChecked(query.ProjectVersioningId);
            user.Forbidden(versioning.UserId != user.UserIdAuthorized, "not owner of versioning");

            // var cacheKey = cache.AddOKM(Handle, query.ProjectVersioningId.ToString(),  );

            if (cache.TryGetOKM<string[]>(this, query.ProjectVersioningId.ToString(), out var cached)) return cached;

            throw new NotImplementedException();
            // using (var git = appManager.OpenGitRepo(versioning.GitDocsRepoUrl))
            // {
            //     string[] tags = git.GetAllTags();
            // 
            //     cache.AddOKM(this, query.ProjectVersioningId.ToString(), tags, TimeSpan.FromMinutes(3));
            // 
            //     return tags;
            // }
        }
    }
}
