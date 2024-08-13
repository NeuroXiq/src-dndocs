

//
// todo
// building docfx sites
//

using DNDocs.Api.Client;
using DNDocs.Docs.Api.Shared;
using DNDocs.Job.Web.Application;
using DNDocs.Job.Web.Infrastructure;
using DNDocs.Job.Web.Services;
using DNDocs.Job.Web.Shared;
using DNDocs.Job.Web.Web;
using Vinca.Api;
using Vinca.BufferLogger;
using Vinca.Utils;

var builder = WebApplication.CreateBuilder(args);

ValidateAppsettings.Validate(
    typeof(DJobSettings),
    builder.Configuration);

DJobSettings settings = new DJobSettings();
builder.Configuration.Bind("DJobSettings", settings);

// external
builder.Services.AddDNClient((Action<DNClientOptions>)(o => { o.ServerUrl = settings.DNServerUrl; o.ApiKey = settings.DNApiKey; }));
builder.Services.AddVOSApi();
builder.Services.AddLogging();
builder.Services.AddVBufferLogger(x => x.MaxLogsTreshold = 10000);
builder.Services.AddVNugetRepositoryFacade();
builder.Services.AddDDocsApiClient((Action<DNDocs.Docs.Api.Client.DDocsApiClientOptions>)(o => { o.ApiKey = settings.DDocsApiKey;  o.ServerUrl = settings.DDocsServerUrl; }));

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
app.UseMiddleware<DJobMiddleware>();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};


app.MapGet($"/api/{nameof(DJobApiController.Ping)}", DJobApiController.Ping);
app.MapGet($"/api/{nameof(DJobApiController.PingAuthorized)}", DJobApiController.PingAuthorized);
app.MapPost($"/api/{nameof(DJobApiController.BuildProject)}", DJobApiController.BuildProject);
app.MapGet($"/api/{nameof(DJobApiController.SystemHtml)}", DJobApiController.SystemHtml);


//app.MapGet("/weatherforecast", () =>
//{
//    var forecast = Enumerable.Range(1, 5).Select(index =>
//        new WeatherForecast
//        (
//            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            Random.Shared.Next(-20, 55),
//            summaries[Random.Shared.Next(summaries.Length)]
//        ))
//        .ToArray();
//    return forecast;
//});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
