using DNDocs.Job.Api.Management;
using DNDocs.Job.Web.Services;
using DNDocs.Job.Web.Shared;
using DNDocs.Job.Web.ValueTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
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
        public static async Task<IResult> Ping(HttpContext httpContext, [FromServices] IApiControllerCtx context)
        {
            return Results.Ok();
        }

        public static async Task<IResult> PingAuthorized(HttpContext httpContext, [FromServices] IApiControllerCtx context)
        {
            XApiKey.Validate(httpContext, context.Settings.DJobApiKey, context.Logger);

            return Results.Ok();
        }

        internal static async Task<IResult> BuildProject(HttpContext context, [FromServices] IApiControllerCtx ctx, [FromBody] BuildProjectModel model)
        {
            XApiKey.Validate(context, ctx.Settings.DJobApiKey, ctx.Logger);

            // do very basic validation only for safety reason
            VValidate.Throw(model.ProjectId < 1, "Id");
            VValidate.Throw(string.IsNullOrWhiteSpace(model.ProjectName), "ProjectName");
            VValidate.Throw(model.ProjectType != ProjectType.NugetOrg && string.IsNullOrWhiteSpace(model.UrlPrefix), "UrlPrefix");
            VValidate.Throw(string.IsNullOrWhiteSpace(model.NugetOrgPackageName) && model.ProjectType == ProjectType.NugetOrg, "NugetOrgPackageName");
            VValidate.Throw(string.IsNullOrWhiteSpace(model.NugetOrgPackageVersion) && model.ProjectType == ProjectType.NugetOrg, "NugetOrgPackageVersion");
            VValidate.Throw(!Enum.IsDefined(model.ProjectType), "ProjectType");
            VValidate.Throw(model.ProjectType == ProjectType.Version && model.PVProjectVersioningId < 1, "PVProjectVersioningId");
            VValidate.Throw(model.ProjectType != ProjectType.NugetOrg && model.ProjectNugetPackages == null || model.ProjectNugetPackages.Count == 0, "nugetpackages");

            if (await ctx.BgJobsService.TryQueueBuildProjectAsync(model))
            {
                return Results.Ok();
            }
            else return Results.StatusCode((int)HttpStatusCode.TooManyRequests);
        }

        internal static async Task SystemHtml(HttpContext context)
        {

        }

        private void SimpleTable<T>(string[] cols, Func<T, string>[] format, IEnumerable<T> values)
        {
            string[,] formatted = new string[cols.Length, values.Count()];


        }
    }
}
