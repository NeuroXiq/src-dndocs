using DNDocs.Docs.Web.Application;
using DNDocs.Docs.Web.Infrastructure;
using DNDocs.Docs.Web.Services;
using DNDocs.Docs.Web.Shared;
using DNDocs.Docs.Web.Web;
using Microsoft.Extensions.Options;
using Vinca.BufferLogger;
using Vinca.Http.Cache;
using Vinca.Http.Logs;
using Vinca.Utils;

namespace DNDocs.Docs.Web
{
    public class Program
    {
        static void AddDNDocsDocsOptions(WebApplicationBuilder builder)
        {
            var optionsBuilder = builder.Services.AddOptions<DOptions>().Bind(builder.Configuration.GetSection($"{nameof(DOptions)}"));

            optionsBuilder
                .Validate(o => Directory.Exists(o.DataDirectory), $"{nameof(DOptions)}.{nameof(DOptions.DataDirectory)}")
                .Validate(o => !string.IsNullOrWhiteSpace(o.Strings?.UrlNugetProjectGenerate), $"{nameof(DOptions)}.{nameof(DOptions.Strings.UrlNugetProjectGenerate)}")
                .Validate(o => !string.IsNullOrWhiteSpace(o.Strings?.UrlProjectNugetOrgFormat), $"{nameof(DOptions)}.{nameof(DOptions.Strings.UrlProjectNugetOrgFormat)}")
                .Validate(o => !string.IsNullOrWhiteSpace(o.Strings?.UrlDDocs), $"{nameof(DOptions)}.{nameof(DOptions.Strings.UrlDDocs)}")
                .Validate(o => !string.IsNullOrWhiteSpace(o.DDocsApiKey), $"{nameof(DOptions)}.{nameof(DOptions.DDocsApiKey)}")
                .Validate(o => o.TimeSpanSaveMetrics.TotalSeconds > 0, $"{nameof(DOptions)}.{nameof(DOptions.TimeSpanSaveMetrics)}")
                .Validate(o => o.FlushAllLogsTimeSpan.TotalSeconds > 0, $"{nameof(DOptions)}.{nameof(DOptions.FlushAllLogsTimeSpan)}")
                .Validate(o => o.TimespanGenerateSitemapPeriod.TotalSeconds > 0, $"{nameof(DOptions)}.{nameof(DOptions.TimespanGenerateSitemapPeriod)}")
                .ValidateOnStart();
        }

        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddJsonFile("secrets.json", optional: false);

            // Add services to the container.
            var settings = new DOptions();

            // config

            AddDNDocsDocsOptions(builder);

            // .net/nuget
            builder.Services.AddMetrics();
            // builder.Services.AddResourceMonitoring();
            builder.Services.AddLogging();
            builder.Services.AddMemoryCache(o =>
            {
                o.SizeLimit = 1024 * 1024 * settings.MemoryCacheMaxSizeMB;
                o.TrackStatistics = true;
                o.CompactionPercentage = 0.3;
            });

            // vinca
            builder.Services.AddVOSApi();
            
            builder.Services.AddVHttpLogs(o =>
            {
                o.ShouldSaveLog = IgnoreHttpLogsFor;
            });

            builder.Services.AddVCacheControlService(o =>
            {
                // todo
                // o.GetCacheResourceInfo = (ctx) => { ctx.RequestServices.GetRequiredKeyedService<> };
            });

            //dndocs.docs services
            builder.Services.AddSingleton<IDMemCache, DMemCache>();
            builder.Services.AddSingleton<IDMetrics, DMetrics>();
            builder.Services.AddTransient<IManagementControllerContext, ManagementControllerContext>();
            builder.Services.AddHostedService<DHostedService>();
            builder.Services.AddDNDDInfrastructure();
            builder.Services.AddVBufferLogger(x =>
            {
                x.MaxLogsTreshold = 10000;
            });

            builder.Services.AddSingleton<ILogsService, LogsService>();
            builder.Services.AddScoped<IManagementService, ManagementService>();
            builder.Services.AddSingleton<IQRepository, QRepository>();
            builder.Services.AddScoped<ITxRepository, TxRepository>();

#if DEBUG
            // add validation service acn be resolved
#endif

            var app = builder.Build();
            app.DInfrastructureAppBuilded();

            app.UseVHttpLogs();
            app.UseVHttpExceptions();
            app.UseHttpsRedirection();
            // app.UseVRateLimit()
            app.UseVCacheControl();

            var allEndpoints = new List<ApiEndpoint>();
            allEndpoints.AddRange(ManagementController.Endpoints);
            allEndpoints.AddRange(PublicContentController.GetEndpoints(app.Services.GetRequiredService<IOptions<DOptions>>()));

            foreach (var e in allEndpoints)
            {
                if (e.HttpMethod == HttpMethod.Get) app.MapGet(e.Route, e.Delegate);
                else if (e.HttpMethod == HttpMethod.Post) app.MapPost(e.Route, e.Delegate).DisableAntiforgery();
                else throw new Exception($"Startup exception, unknown HttpMethod to bind: '{e.HttpMethod?.ToString()}' on route '{e.Route}'");
            }


            //byte[] filebytes = File.ReadAllBytes(@"C:\Users\user\Desktop\ef_site.zip");

            // #if DEGUB
            //for (int i = 0; i < 1; i++)
            //{
            //    Task.Factory.StartNew(async () =>
            //    {
            //        var id = new Random().Next(1000123);
            //        // var stre = new FileStream(@"C:\Users\user\Desktop\_site.zip", FileMode.Open);
            //        var stre = new MemoryStream();
            //        stre.Write(filebytes);
            //        stre.Position = 0;
            //        stre.Position = 0;
            //        new DndocsDocsApiClient(new DndocsApiClientOptions { ApiKey = "@T4hjr4dsh$%H$J%45j6t7kY^zsdg34", ServerUrl = "https://localhost:7088" }, new asdf())
            //        .CreateOrReplaceProject(id, "pname", null, null, id.ToString(), id.ToString(), 3, "dndocs-ver1",
            //        stre).Wait();
            //    });
            //}
            
            // #endif
           
            
           app.Run();
        }


        static bool IgnoreHttpLogsFor(HttpContext context)
        {
            // ignore hot paths as there is no use of logs for e.g. '.js/.css' files when 99.99% will be 200 OK
            var path = context.Request.Path.Value;
            if ((context.Response.StatusCode == 200 || context.Response.StatusCode == 304) && path != null)
            {
                if (
                path.StartsWith("/n/") ||
                path.StartsWith("/v/") ||
                path.StartsWith("/s/"))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
