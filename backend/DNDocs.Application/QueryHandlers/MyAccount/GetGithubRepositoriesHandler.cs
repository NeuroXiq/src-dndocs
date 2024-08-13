using Microsoft.Extensions.Logging;
using DNDocs.Application.Queries.MyAccount;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Utils;
using DNDocs.Infrastructure.DomainServices;
using DNDocs.Infrastructure.Utils;
using DNDocs.Api.DTO.MyAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.QueryHandlers.MyAccount
{
    internal class GetGithubRepositoriesHandler : QueryHandlerA<GetGithubRepositoriesQuery, IList<GithubRepositoryDto>>
    {
        private ICache cache;
        private IGithubAPI githubApi;
        private ICurrentUser user;

        public GetGithubRepositoriesHandler(
            ICurrentUser user,
            IGithubAPI githubApi,
            ICache cache)
        {
            this.cache = cache;
            this.githubApi = githubApi;
            this.user = user;
        }

        protected override async Task<IList<GithubRepositoryDto>> Handle(GetGithubRepositoriesQuery query)
        {
            User userEntity = user.User;

            try
            {
                if (!string.IsNullOrWhiteSpace(userEntity.GithubLogin))
                {
                    if (!query.FlushCache && cache.TryGetJKM<IList<GithubRepositoryDto>>(this, userEntity.GithubLogin, out var cachedResult))
                    {
                        return cachedResult;
                    }

                    var repos = await githubApi.GetUserRepositories(userEntity.GithubLogin);

                    var result = repos
                        .Where(t => !t.fork)
                        .OrderBy(t => t.name)
                        .Select(t => new GithubRepositoryDto() { Name = t.name, GitUrl = t.git_url, CloneUrl = t.clone_url })
                        .ToList();

                    cache.AddJKM(this, userEntity.GithubLogin, result, TimeSpan.FromDays(4));

                    return result;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "ignoring error, will return empty list, log for future maintenance");
            }

            return new List<GithubRepositoryDto>();
        }
    }
}
