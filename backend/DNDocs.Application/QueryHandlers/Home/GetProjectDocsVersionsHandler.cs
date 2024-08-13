using Microsoft.EntityFrameworkCore;
using DNDocs.Application.Queries.Home;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Api.DTO.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.QueryHandlers.Home
{
    internal class GetProjectDocsVersionsHandler : QueryHandlerA<GetProjectDocsVersionsQuery, IList<ProjectDocsVersionDto>>
    {
        private IAppUnitOfWork uow;

        public GetProjectDocsVersionsHandler(IAppUnitOfWork uow)
        {
            this.uow = uow;
        }

        protected override async Task<IList<ProjectDocsVersionDto>> Handle(GetProjectDocsVersionsQuery query)
        {
            var projs = await uow.GetSimpleRepository<Project>().Query()
                .Where(t => t.PVProjectVersioningId == query.ProjectVersioningId)
                .OrderByDescending(t =>  t.PVGitTag)
                .Select(t => new { t.UrlPrefix, t.PVGitTag })
                .ToListAsync();

            var dtos = projs.Select(t => new ProjectDocsVersionDto() { GitTagName = t.PVGitTag, ProjectUrlPrefix = t.UrlPrefix }).ToList();

            return dtos;
        }
    }
}
