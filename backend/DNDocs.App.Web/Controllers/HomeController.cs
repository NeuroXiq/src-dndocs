using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using DNDocs.Application.Queries.Home;
using DNDocs.Application.Shared;
using DNDocs.Shared.Configuration;

using DNDocs.Web.Application;
using DNDocs.Web.Models.Home;
using DNDocs.Api.DTO;
using System.Diagnostics;
using System.Net;
using System.Reflection;

namespace DNDocs.Web.Controllers
{
    public class HomeController : ApiControllerBase
    {
        private readonly ILogger<HomeController> logger;
        private readonly ICommandDispatcher cd;
        private readonly IQueryDispatcher qd;
        private readonly DNDocsSettings rsettings;
        private readonly IRobiniaResources res;

        public HomeController(
            ILogger<HomeController> logger,
            IRobiniaResources res,
            IOptions<DNDocsSettings> options,
            IQueryDispatcher qd,
            ICommandDispatcher cd)
        {
            this.logger = logger;
            this.cd = cd;
            this.qd = qd;
            this.rsettings = options.Value;
            this.res = res;
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectDocsVersions(int projectVersioningId)
        {
            return await ApiResult2(qd.DispatchAsync(new GetProjectDocsVersionsQuery() { ProjectVersioningId = projectVersioningId }));
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentProjects()
        {
            return await ApiResult2(qd.DispatchAsync(new GetRecentProjectsQuery()));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            return await ApiResult2(qd.DispatchAsync(new GetAllProjectsQuery()));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("/UseExceptionHandler")]
        public IActionResult Error()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionFeature != null)
            {
                var originalPathAndQuery = exceptionFeature.Path;
                var routeValues = "";

                var vals = exceptionFeature.RouteValues?.Select(t => "KEY:" + (t.Key ?? "") + "\tVALUE:" + (t.Value ?? "")).ToArray();
                vals = vals ?? new string[0];
                routeValues = string.Join("\r\n", vals);

                var iseException = exceptionFeature.Error;
                var iseLogMessage = string.Format(
                    "EndpointDisplayName: {0}\r\n" +
                    "ErrorMessage: {1}\r\n" +
                    "ErrorStackTrace: {2}\r\n" +
                    "Error: {3}\r\n" +
                    "Path: {4}\r\n" +
                    "RouteValues: {5}\r\n",
                    exceptionFeature.Endpoint?.DisplayName?.ToString() ?? "",
                    exceptionFeature.Error?.Message?.ToString() ?? "",
                    exceptionFeature.Error?.StackTrace?.ToString() ?? "",
                    exceptionFeature.Error?.ToString() ?? "",
                    exceptionFeature.Path?.ToString() ?? "",
                    routeValues
                    );

                this.logger.LogCritical(iseException, iseLogMessage);
            }

            return UseStatusCodePagesWithReExecute(500);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("/UseStatusCodePagesWithReExecute")]
        public IActionResult UseStatusCodePagesWithReExecute(int statuscode)
        {
            
            this.logger.Log(LogLevel.Warning, "Entering UseStatusCodePagesWithReExecute, statuscode=" + statuscode);

            if (statuscode == (int)HttpStatusCode.Unauthorized)
                return Redirect("/auth/login");

            var feature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            
            string originalPathAndQuery = "";

            if (statuscode == 404)
            {
                if (HttpContext.Items.TryGetValue("NotFoundViewModel", out var vmAsJson))
                {
                    try
                    {
                        // var additionalDataVm = JsonSerializer.Deserialize<ObjectNotFoundModel>(vmAsJson as string);

                        // return View("_ObjectNotFound", additionalDataVm);
                    }
                    catch
                    {
                    }
                }

                originalPathAndQuery = string.Join(
                    feature.OriginalPathBase,
                    feature.OriginalPath,
                    feature.OriginalQueryString);
            }

            //var vm = new StatusCodeReExecuteViewModel(statuscode,
            //    "Status Code",
            //    "Something was wrong with request",
            //    "");

            //int[] definedResources = new int[] { 403, 404, 500 };

            //if (definedResources.Contains(statuscode))
            //{
            //    vm = new StatusCodeReExecuteViewModel(
            //        statuscode,
            //        res[$"StatusCode_{statuscode}_Title"],
            //        res[$"StatusCode_{statuscode}_Description"],
            //        originalPathAndQuery?.Trim());
            //}

            //if (statuscode == 500)
            //{
            //    vm.Data = res["StatusCode_500_Data"];
            //}

            //return new StatusCodeResult(500);

            return new StatusCodeResult(500);
            // return View("UseStatusCodePagesWithReExecute", vm);
        }

        [Route("/api/system")]
        public Task<IActionResult> System()
        {
            var ver = FileVersionInfo.GetVersionInfo((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location);

            var verinfo = new
            {
                ver.FileVersion,
                ver.Comments,
                ver.CompanyName,
                ver.FileDescription,
                ver.ProductName,
                ver.ProductVersion,
                ver.LegalCopyright,
                EnvironmentVersion = Environment.Version.ToString()
            };

            return ApiResult2(Task.FromResult(new QueryResult<object>(verinfo)));
        }


    }
}