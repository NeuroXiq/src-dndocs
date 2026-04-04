

//
using DNDocs.Api.Client;
using DNDocs.Docs.Api.Shared;
using DNDocs.Job.Web.Application;
using DNDocs.Job.Web.Infrastructure;
using DNDocs.Job.Web.Services;
using DNDocs.Job.Web.Shared;
using DNDocs.Job.Web.Web;
using Vinca.BufferLogger;
using Vinca.Utils;

namespace DNDocs.Job.Web
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddJsonFile("secrets.json", optional: false);

            AddDNDocsJobOptions(builder);

            // external
            builder.AddDNDocsApiClient();
            builder.Services.AddVOSApi();
            builder.Services.AddLogging();
            builder.Services.AddVBufferLogger(x => x.MaxLogsTreshold = 10000);
            builder.Services.AddVNugetRepositoryFacade();
            builder.AddDNDocsDocsApiClient();

            // djob
            builder.Services.AddScoped<IDocsBuilderService, DocsBuilderService>();
            builder.Services.AddScoped<IDocfxManager, DocfxManager>();
            builder.Services.Configure<DJobSettings>(builder.Configuration.GetSection($"{nameof(DJobSettings)}"));
            builder.Services.AddSingleton<IDJobInfrastructure, DJobInfrastructure>();
            builder.Services.AddSingleton<IBgJobsService, BgJobsService>();
            builder.Services.AddSingleton<IDJobRepository, DJobRepository>();
            builder.Services.AddScoped<IApiControllerCtx, ApiControllerCtx>();
            builder.Services.AddHostedService<DJobHostedService>();

            var app = builder.Build();

            app.Services.GetRequiredService<IDJobInfrastructure>().Startup();

            // Configure the HTTP request pipeline.

            app.UseVHttpExceptions();
            app.UseHttpsRedirection();

            // map routes
            app.MapGet($"/api/system/health/", DJobApiController.Health);
            app.MapGet($"/api/{nameof(DJobApiController.Ping)}", DJobApiController.Ping);
            app.MapGet($"/api/{nameof(DJobApiController.PingAuthorized)}", DJobApiController.PingAuthorized);
            app.MapPost($"/api/build-nugetorg-project", DJobApiController.BuildNugetOrgProject);
            app.MapGet($"/api/{nameof(DJobApiController.System)}", DJobApiController.System);

            app.Run();
        }

        static void AddDNDocsJobOptions(this WebApplicationBuilder builder)
        {
            var optionsBuilder = builder.Services.AddOptions<DJobSettings>().Bind(builder.Configuration.GetSection(nameof(DJobSettings)));

            optionsBuilder
                .Validate(o => !string.IsNullOrWhiteSpace(o.DJobApiKey), nameof(DJobSettings.DJobApiKey))
                .Validate(o => !string.IsNullOrWhiteSpace(o.OSPathInfrastructureDirectory), nameof(DJobSettings.OSPathInfrastructureDirectory))
                .Validate(o => o.MaxParallelBuildCount > 0, $"{nameof(DJobSettings.MaxParallelBuildCount)} must be > 0")
                .ValidateOnStart();
        }
    }
}

