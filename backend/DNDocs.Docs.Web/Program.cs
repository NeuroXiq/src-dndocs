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
                .Validate(o =>
                    !string.IsNullOrWhiteSpace(o.Strings?.UrlDNDocsDocs)
                    && !o.Strings.UrlDNDocsDocs.EndsWith("/"),
                    $"{nameof(DOptions)}.{nameof(DOptions.Strings.UrlDNDocsDocs)} empty or ends with '/'")

                .Validate(o => !string.IsNullOrWhiteSpace(o.DNDocsDocsApiKey), $"{nameof(DOptions)}.{nameof(DOptions.DNDocsDocsApiKey)}")
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
            
           app.Run();
        }


        static bool IgnoreHttpLogsFor(HttpContext context)
        {
            // ignore hot paths as there is no use of logs for e.g. '.js/.css' files when 99.99% will be 200 OK
            var path = context.Request.Path;

            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                bool shouldIgnore = path.StartsWithSegments("/n") ||
                    path.StartsWithSegments("/favicon.ico") ||
                    path.StartsWithSegments("/public") ||
                    path.StartsWithSegments("/robots.txt");

                return shouldIgnore;
            }

            return false;
        }
    }
}
