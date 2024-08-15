using DNDocs.Docs.Api.Management;
using DNDocs.Docs.Web.Infrastructure;
using DNDocs.Docs.Web.Services;
using DNDocs.Docs.Web.Shared;
using DNDocs.Docs.Web.ValueTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Text;
using Vinca.Api;
using Vinca.Utils;

namespace DNDocs.Docs.Web.Web
{
    public interface IManagementControllerContext
    {
        ILogger<ManagementController> Logger { get; }
        IOptions<DSettings> Settings { get; }
        IOSApi OSApi { get; }
        IManagementService ManagementService { get; }
    }

    public class ManagementControllerContext : IManagementControllerContext
    {
        public IOSApi OSApi { get; set; }

        public ILogger<ManagementController> Logger { get; set; }

        public HttpContext HttpContext { get; set; }

        public IOptions<DSettings> Settings { get; set; }

        public IManagementService ManagementService { get; set; }

        public ManagementControllerContext(ILogger<ManagementController> logger,
            IOptions<DSettings> settings,
            IManagementService managementService,
            IOSApi osApi)
        {
            Logger = logger;
            Settings = settings;
            ManagementService = managementService;
            OSApi = osApi;
        }
    }

    public class ManagementController
    {
        public const string Controller = "/api/management";

        public static readonly ApiEndpoint[] Endpoints = new ApiEndpoint[]
        {
            GetEndpoint(HttpMethod.Get, "/ping/{reply?}", Ping),
            GetEndpoint(HttpMethod.Post, "/createproject", CreateProject),
            GetEndpoint(HttpMethod.Get, "/deleteproject", DeleteProject),
            GetEndpoint(HttpMethod.Get, "/metrics/{seconds:int?}", Metrics),
        };


        static async Task<IResult> Metrics(
            HttpContext context,
            [FromServices] IQRepository qrepository,
            [FromServices] IManagementControllerContext mmc,
            [FromRoute] int? seconds)
        {
            int secondsBefore = seconds.HasValue ? seconds.Value : 600;

            var metrics = await qrepository.SelectMtMeasurementSum(DateTime.UtcNow.AddSeconds(-secondsBefore), DateTime.UtcNow);
            var sb = new StringBuilder();
            PublicContentController.AppendHtmlTable(sb,
                new[] { "IID", "Sum", "MtInstrumentName", "MtHRangeEnd", "Tags", "Type" },
                new Func<MtMeasurementSum, object>[]
                {
                    t => t.InstrumentId,
                    t => string.Format("{0:n}", t.Sum),
                    t => t.InstrumentName,
                    t => t.MtHRangeEnd,
                    t => t.InstrumentTags,
                    t => t.InstrumentType
                },
                metrics);

            return PublicContentController.SimpleHtmlPage(sb);
        }

        private static async Task<IResult> DeleteProject(
            HttpContext context,
            [FromBody]DeleteProjectModel model,
            [FromServices] IManagementControllerContext mmc)
        {
            Authorized(context, mmc);
            await mmc.ManagementService.DeleteProject(model.ProjectId);

            return Results.Ok();
        }

        static ApiEndpoint GetEndpoint(HttpMethod method, string path, Delegate delegateMethod)
        {
            return new ApiEndpoint(method, $"{Controller}{path}", delegateMethod);
        }
        
        static async Task<IResult> CreateProject([FromForm] CreateProjectModel m,
            HttpContext context,
            [FromServices] IManagementControllerContext mmc)
        {
            Authorized(context, mmc);


            using var tempFile = mmc.OSApi.CreateTempFile();
            using var stream = File.Create(tempFile.OSFullPath);
            await m.SiteZip.CopyToAsync(stream);

            await mmc.ManagementService.CreateProject(
                m.ProjectId,
                m.Metadata,
                m.ProjectName,
                m.UrlPrefix,
                m.PVVersionTag,
                m.NPackageName,
                m.NPackageVersion,
                (Model.ProjectType)m.ProjectType,
                stream);

            return Results.Ok();
        }

        static IResult Ping(
            HttpContext context,
            [FromServices] IManagementControllerContext mcContext,
            string reply)
        {
            Authorized(context, mcContext);

            return Results.Content(reply ?? "", "text/plain; charset=UTF-8");
        }

        private static void Authorized(HttpContext context, IManagementControllerContext mcContext)
        {
            var logger = mcContext.Logger;
            var apiKey = mcContext.Settings.Value.DDocsApiKey;

            Exception exc = null;

            try
            {
                if (context.Request.Headers.TryGetValue("x-api-key", out var apiKeyHeader))
                {
                    if (apiKeyHeader.Count == 1 && apiKeyHeader[0] == apiKey)
                    {
                        return;
                    }
                }

                logger.LogWarning("unauthorized, path: {0}, remote ip: {1}, remote port: {2}, headers:\r\n{3} ",
                    context.Request.Path,
                    context.Connection.RemoteIpAddress.ToString(),
                    context.Connection.RemotePort,
                    context.Request.Headers.Select(t => $"{t.Key}: {t.Value.StringJoin(", ")}\r\n"));

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            catch (Exception e)
            {
                exc = e;
            }

            throw new DUnauthorizedException();
        }
    }
}
