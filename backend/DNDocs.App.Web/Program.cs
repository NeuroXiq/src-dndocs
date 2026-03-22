using DNDocs.App.Domain.Service;
using DNDocs.Application.Utils;
using DNDocs.Docs.Api.Client;
using DNDocs.Docs.Api.Shared;
using DNDocs.Domain.Entity;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Infrastructure.DataContext;
using DNDocs.Infrastructure.UnitOfWork;
using DNDocs.Infrastructure.Utils;
using DNDocs.Job.Api.Client;
using DNDocs.Shared.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Vinca.Api;
using Vinca.Ddns;
using Vinca.DDNS;
using Vinca.Exceptions;
using Vinca.Http.Logs;
using Vinca.Utils;
using static DNDocs.Infrastructure.Utils.RawRobiniaInfrastructure;

namespace DNDocs.Web
{
    public class Program
    {
        /// <summary>
        /// Method used to start server for integration tests, start this method on separate thread
        /// using this method and run integration tests
        /// </summary>
        public static void ITMain() { Main(new string[] { "IntegrationTests" }); }

        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();

            if (builder.Environment.IsDevelopment()) builder.Logging.AddConsole();

            if (builder.Environment.EnvironmentName == "IntegrationTests" || args.Contains("IntegrationTests"))
            {
                builder.Configuration.AddJsonFile("appsettings.Development.json", optional: false);
                builder.Configuration.AddJsonFile("appsettings.IntegrationTests.json", optional: false);
            }

            var dnOptions = AddDNOptions(builder);

            var services = builder.Services;

            // Add services to the container.

            // asp.net framework/nuget
            builder.Services.AddControllersWithViews();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = dnOptions.Jwt.Issuer,
                    ValidAudience = dnOptions.Jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(dnOptions.Jwt.GetBytes_SymmetricSecurityKey()),
                };

                opt.Validate();
            });
            builder.Services.AddAutoMapper(typeof(Program).Assembly);

            // vinca

            // vinca-ddns
            builder.AddVDdnsPorkbunService();
            builder.AddVDdnsHostedService(c => c.RefreshDdnsPeriod = TimeSpan.FromHours(24));

            services.AddDJobClientFactory();
            services.AddVNugetRepositoryFacade();
            services.AddVHttpLogs(c => c.MaxQueueSize = 10000);
            builder.AddVIndexNowApi();

            // dndocs

            services.AddOptions<OptionsDDocsApiClient>().Bind(builder.Configuration.GetSection($"{nameof(OptionsDDocsApiClient)}"));
            services.AddDDocsApiClient();

            services.AddHttpClient();
            services.Configure<CookiePolicyOptions>(opt =>
            {
                opt.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });
            
            builder.Services.Configure<FormOptions>(opt =>
            {
                // 16 Megabytes limit for all forms in system
                opt.MultipartBodyLengthLimit = 16 * 1024 * 1024;
            });

            builder.Services.AddRobiniaInfrastructure(dnOptions.OSPathInfrastructureDirectory);

            // dndocs.app.domain
            services.AddScoped<AppDbContext>();
            services.AddScoped<INugetOrgProjectService, NugetOrgProjectService>();
            services.AddScoped<IAppUnitOfWork, AppUnitOfWork>();

            StartupRobiniaApplication.AddRobiniaApplication(builder);

            var app = builder.Build();

            DeploySetup(app);

            // todo remove this when get rid of node.js server when vite
            var fho = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All,
            };

            fho.KnownNetworks.Clear();
            fho.KnownProxies.Clear();

            app.UseCors(c => c.WithOrigins(dnOptions.CorsAllowedOrigins)
                .AllowAnyMethod()
                .AllowCredentials()
                .AllowAnyHeader());

            app.UseForwardedHeaders(fho);
            app.UseVHttpLogs();
            app.UseVHttpExceptions();
            app.Use(async (context, next) =>
            {
                // temp solution
                try
                {
                    await next(context);
                }
                catch (DNDomainException e)
                {
                    throw new VValidationException(e.Message);
                }
                catch (Exception e)
                {

                    throw;
                }
                catch { throw; }
            });

            app.Use(async (context, next) =>
            {
                var token = context.Request.Cookies["Token"];

                if (!string.IsNullOrEmpty(token) &&
                    !context.Request.Headers.ContainsKey("Authorization"))
                {
                    context.Request.Headers.Append("Authorization", "Bearer " + token);
                }

                await next();
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
                // app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRouting();
            app.UseAuthorization();
            app.UseAuthentication();
            app.UseStaticFiles();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        private static DNDocsSettings AddDNOptions(WebApplicationBuilder builder)
        {
            var temp = new DNDocsSettings();
            builder.Configuration.GetSection("DNDocsSettings").Bind(temp);

            var services = builder.Services;
            var dnOptionsBuilder = services.AddOptions<DNDocsSettings>();

            services.Configure<DNDocsSettings>(builder.Configuration.GetSection($"{nameof(DNDocsSettings)}"));

            dnOptionsBuilder
                .Validate(c => !string.IsNullOrWhiteSpace(c.DDocsApiKey), "DDocsApiKey")
                .Validate(c => !string.IsNullOrWhiteSpace(c.DJobApiKey), "DJobApiKey")
                .Validate(c => !string.IsNullOrWhiteSpace(c.DNApiKey), "DNApiKey")
                .Validate(c => !string.IsNullOrWhiteSpace(c.AdminPasswordSha512), "AdminPasswordSha512")
                .Validate(c => c.BackendBackgroundWorkerDoImportantWorkSleepSeconds > 5, "BackendBackgroundWorkerDoImportantWorkSleepSeconds")
                .Validate(c => c.BackendBackgroundWorkerDoWorkSleepSeconds > 5, "BackendBackgroundWorkerDoWorkSleepSeconds")
                .Validate(c => c.FrontendBackgroundWorkerDoWorkSleepSeconds > 5, "FrontendBackgroundWorkerDoWorkSleepSeconds")
                .Validate(c => !string.IsNullOrWhiteSpace(c.OSPathInfrastructureDirectory), "OSPathInfrastructureDirectory")
                .Validate(c => c.CorsAllowedOrigins?.Length > 0, "CorsAllowedOrigins")
                .Validate(c => !string.IsNullOrWhiteSpace(c.Jwt.Issuer), "Issuer")
                .Validate(c => !string.IsNullOrWhiteSpace(c.Jwt.Audience), "Audience")
                .Validate(c => !string.IsNullOrWhiteSpace(c.Jwt.SymmetricSecurityKey), "SymmetricSecurityKey")
                .Validate(c => !string.IsNullOrWhiteSpace(c.UrlProjectNugetOrgApiFolder), "UrlProjectNugetOrgApiFolder")
                .ValidateOnStart();

            // custom validations
            
            if (!Directory.Exists(temp.OSPathInfrastructureDirectory))
            {
                throw new Exception($"directory not exists: '{temp.OSPathInfrastructureDirectory}'");
            }

            return temp;
        }

        static void ThrowStartupException(bool doThrow, string message)
        {
            string msg = "Startup Exception (e.g. example appsettings.json)\r\n";
            msg += message;

            if (doThrow)
            {
                throw new Exception(msg);
            }
        }

        private static void DeploySetup(WebApplication app)
        {
            var services = app.Services;
            services.GetRequiredService<IDNInfrastructure>().RunAppMigrations();

            using (var scope = services.CreateScope())
            {
                var appUow = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>();

                var userRepo = appUow.GetSimpleRepository<User>();

                if (app.Environment.EnvironmentName == "IntegrationTests")
                {
                    if (!userRepo.Query().Where(t => t.Login == User.User1LoginIntegrationTests).Any())
                    {
                        var adminUser = new User(User.User1LoginIntegrationTests, "IntegrationTestUser1@robiniadocs.com");
                        userRepo.Create(adminUser);
                    }

                    if (!userRepo.Query().Where(t => t.Login == User.User2LoginIntegrationTests).Any())
                    {
                        var adminUser = new User(User.User2LoginIntegrationTests, "IntegrationTestUser2@robiniadocs.com");
                        userRepo.Create(adminUser);
                    }
                }

                var requiredDefaultUsers = new string[] { User.AdministratorUserLogin, User.RobiniaAppServiceUserLogin, User.NuGetUserLogin };

                foreach (var loginToAdd in requiredDefaultUsers)
                {
                    if (!userRepo.Query().Where(t => t.Login == loginToAdd).Any())
                    {
                        var adminUser = new User(loginToAdd, $"{loginToAdd}@dndocs.com");
                        userRepo.Create(adminUser);
                    }
                }

                appUow.SaveChanges();
            }
        }
    }
}