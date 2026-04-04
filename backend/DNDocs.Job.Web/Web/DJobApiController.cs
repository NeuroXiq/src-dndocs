using DNDocs.Job.Api.Management;
using DNDocs.Job.Web.Services;
using DNDocs.Job.Web.Shared;
using DNDocs.Job.Web.ValueTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using Vinca.Exceptions;
using Vinca.Http;
using Vinca.Utils;

namespace DNDocs.Job.Web.Web
{
    public interface IApiControllerCtx
    {
        ILogger Logger { get; }
        IBgJobsService BgJobsService { get; }
        DJobSettings Settings { get; }
    }

    public class ApiControllerCtx : IApiControllerCtx
    {
        public ILogger Logger { get; set; }
        public DJobSettings Settings { get; set; }
        public IBgJobsService BgJobsService { get; set; }

        public ApiControllerCtx(
            IOptions<DJobSettings> options,
            ILogger<DJobApiController> logger,
            IBgJobsService bgJobsService)
        {
            Logger = logger;
            Settings = options.Value;
            BgJobsService = bgJobsService;
        }
    }

    public class DJobApiController
    {
        public static async Task<IResult> Health(HttpContext httpContext, [FromServices] IApiControllerCtx context)
        {
            return Results.Json(SystemHealthApiResponse.Create(typeof(DNDocs.Job.Web.Program).Assembly));
        }

        public static async Task<IResult> Ping(HttpContext httpContext, [FromServices] IApiControllerCtx context)
        {
            return Results.Ok();
        }

        public static async Task<IResult> PingAuthorized(HttpContext httpContext, [FromServices] IApiControllerCtx context)
        {
            XApiKey.Validate(httpContext, context.Settings.DJobApiKey, context.Logger);

            return Results.Ok();
        }

        internal static async Task<IResult> BuildNugetOrgProject(HttpContext context, [FromServices] IApiControllerCtx ctx, [FromBody] BuildNugetOrgProjectModel model)
        {
            XApiKey.Validate(context, ctx.Settings.DJobApiKey, ctx.Logger);

            // do very basic validation only for safety reason
            VValidate.Throw(model.ProjectId < 1, "Id");

            if (await ctx.BgJobsService.TryQueueBuildNugetOrgProjectAsync(model))
            {
                return Results.Ok();
            }
            else return Results.StatusCode((int)HttpStatusCode.TooManyRequests);
        }

        internal static IResult System(HttpContext context)
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

            return Results.Json(verinfo);
        }

        private void SimpleTable<T>(string[] cols, Func<T, string>[] format, IEnumerable<T> values)
        {
            string[,] formatted = new string[cols.Length, values.Count()];
        }
    }
}
