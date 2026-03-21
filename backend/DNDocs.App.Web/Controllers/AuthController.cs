using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.IdentityModel.Tokens;
using DNDocs.Application.Commands.Auth;
using DNDocs.Application.Queries.Auth;
using DNDocs.Application.Shared;
using DNDocs.Domain.ValueTypes;
using DNDocs.Shared.Configuration;
using DNDocs.Web.Models.Auth;
using DNDocs.Api.DTO;
using DNDocs.Api.Admin;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DNDocs.Web.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AuthController : ApiControllerBase
    {
        private IQueryDispatcher qd;
        private IAntiforgery antiforgery;
        private IHttpClientFactory httpClientFactory;
        private IConfiguration configuration;
        private DNDocsSettings RobiniaSettings;
        private ICommandDispatcher cd;

        public AuthController(IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IQueryDispatcher qd,
            ICommandDispatcher cd)
        {
            this.cd = cd;
            this.qd = qd;
            this.antiforgery = antiforgery;
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            RobiniaSettings = new DNDocsSettings();
            configuration.GetSection("RobiniaSettings").Bind(RobiniaSettings);
        }

        [HttpPost]
        public async Task<IActionResult> CallbackGithubOAuth([FromBody]string code)
        {
            var githubLoginCommand = new LoginUserCommand.GithubLoginCommand(code);

            return await ApiResult2(cd.DispatchAsync(new LoginUserCommand(githubLoginCommand, null)));
        }

        [HttpPost]
        public async Task<IActionResult> AdminLogin(AdminLoginModel model)
        {
            var cmd = new LoginUserCommand(null, new LoginUserCommand.AdminLoginCommand(model.Login, model.Password));
            return await ApiResult2(Task.FromResult(cd.Dispatch(cmd)));

        }
    }
}
