
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DNDocs.Application.Commands;
using DNDocs.Application.Queries;
using DNDocs.Application.Queries.MyAccount;
using DNDocs.Application.Shared;
using DNDocs.Web.Application.RateLimit;
using DNDocs.Web.Models.MyAccount;
using DNDocs.Api.DTO;

namespace DNDocs.Web.Controllers
{
    //[Route("my-account")]
    [Route("/api/[controller]/[action]")]
    [Authorize]
    public class MyAccountController : ApiControllerBase
    {
        private IMapper mapper;
        private IQueryDispatcher qd;
        private ICommandDispatcher cd;

        public MyAccountController(
            IMapper mapper,
            ICommandDispatcher cd,
            IQueryDispatcher qd)
        {
            this.qd = qd;
            this.cd = cd;
            this.mapper = mapper;
        }

        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> GetGithubRepositories(bool flushCache)
        {
            // TODO must rate limit this to endpoint avoid to many request to www.github.com
            Thread.Sleep(1999);
            return await ApiResult2(qd.DispatchAsync(new GetGithubRepositoriesQuery() { FlushCache = flushCache }));
        }

        [HttpGet]
        public async Task<IActionResult> GetMyProjects()
        {
            return await ApiResult2(qd.DispatchAsync(new GetMyProjectsQuery()));
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountDetails()
        {
            return await ApiResult2(qd.DispatchAsync(new GetMyUserQuery()));
        }

        [HttpGet]
        public async Task<IActionResult> GetSystemMessages(int? projectId, int pageNo, int rowsPerPage, int? projectVersioningId)
        {
            return await ApiResult2(qd.DispatchAsync(new GetUserSystemMessagesQuery(pageNo, rowsPerPage, projectId, projectVersioningId)));
        }

        [HttpGet]
        public async Task<IActionResult> GetBgJob(int projectId)
        {
            return await ApiResult2(qd.DispatchAsync(new GetBgJobQuery() { ProjectId = projectId }));
        }

        //
        // Post
        //

        [RateLimit(RLP.Project)]
        [HttpPost]
        public async Task<IActionResult> ClearCache(string cacheName)
        {
            return null;
        }

        //[HttpPost("MyAccount/RequestProject")]
        //[Route("/api/MyAccount/RequestProject")]
        
    }
}
