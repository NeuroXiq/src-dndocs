using DNDocs.App.Domain.Service;
using DNDocs.Application.Utils;
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

            var services = builder.Services;

            SetupSettings(builder.Configuration);

            var robiniaSettings = new DNDocsSettings();
            builder.Configuration.GetSection("DNDocsSettings").Bind(robiniaSettings);

            // Add services to the container.
            
            // asp.net framework services
            builder.Services.AddControllersWithViews();

            // dndocs.app
            var dsettings = new DNDocsSettings();
            builder.Configuration.GetSection($"{nameof(DNDocsSettings)}").Bind(dsettings);

            services.AddHttpClient();
            services.Configure<CookiePolicyOptions>(opt =>
            {
                opt.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });

            services.AddVIndexNowApi(
                dsettings.IndexNowSubmitUrl,
                dsettings.IndexNowHost,
                dsettings.IndexNowApiKey,
                dsettings.IndexNowKeyLocation);
            
            services.AddVHttpLogs(c => c.MaxQueueSize = 10000);
            services.Configure<DNDocsSettings>(builder.Configuration.GetSection($"{nameof(DNDocsSettings)}"));
            services.AddDJobClientFactory();
            services.AddVNugetRepositoryFacade();
            services.AddDDocsApiClient(o => { o.ApiKey = dsettings.DDocsApiKey; o.ServerUrl = dsettings.DDocsServerUrl; });
            builder.Services.AddAutoMapper(typeof(Program).Assembly);

            builder.Services.Configure<FormOptions>(opt =>
            {
                // 16 Megabytes limit for all forms in system
                opt.MultipartBodyLengthLimit = 16 * 1024 * 1024;
            });

            builder.Services.AddRobiniaInfrastructure(dsettings.OSPathInfrastructureDirectory);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = dsettings.Jwt.Issuer,
                    ValidAudience = dsettings.Jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(dsettings.Jwt.GetBytes_SymmetricSecurityKey()),
                };

                opt.Validate();
            });

            services.AddOptions<DNDocsSettings>()
                .Bind(builder.Configuration.GetSection($"{nameof(DNDocsSettings)}"));


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

            app.UseCors(c => c.WithOrigins(robiniaSettings.CorsAllowedOrigins)
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

        private static void SetupSettings(ConfigurationManager configuration)
        {
            // expected to be in same directory as web.dll (as current executing code)
            // only for safety purpose - throw on startup if something is wrong with settings
            // smoke-test instead of throwing something unexpected in runtime
            var rs = configuration.GetSection($"{nameof(DNDocsSettings)}").Get<DNDocsSettings>();

            var settingsProps = typeof(DNDocsSettings).GetProperties().Where(t => t.PropertyType == typeof(string)).Select(t => t.Name);
            var stringsProps = typeof(DNDocsSettings.StringsSettings).GetProperties().Select(t => $"Strings:{t.Name}");
            var jwtSettingsProps = typeof(DNDocsSettings.JwtSettings).GetProperties().Select(t => $"Jwt:{t.Name}");
            var githubProps = typeof(DNDocsSettings.GithubOAuthSettings).GetProperties().Select(t => $"GithubOAuth:{t.Name}");

            var requiredExists = settingsProps.Union(stringsProps).Union(jwtSettingsProps).Union(githubProps).ToArray();

            foreach (var required in requiredExists)
            {
                var fullpath = $"{nameof(DNDocsSettings)}:{required}";

                ThrowStartupException(
                    string.IsNullOrWhiteSpace(configuration.GetValue<string>(fullpath)),
                    $"Invalid setting (must not be empty): {fullpath}");
            }

            ThrowStartupException(!Directory.Exists(rs.OSPathInfrastructureDirectory),
                $"RobiniaSettings: {nameof(rs.OSPathInfrastructureDirectory)} does not exists." +
                "Create this directory to setup/deploy project or change appsettings to other location.");
            ThrowStartupException(!File.Exists(rs.GitExeFilePath), $"git.exe file does not exist. Provide valid git.exe full OS Path. current path (invalid): '{rs.GitExeFilePath}'");
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