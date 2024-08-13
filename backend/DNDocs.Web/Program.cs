using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using DNDocs.Application.Application;
using DNDocs.Application.Utils;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.ValueTypes;
using DNDocs.Infrastructure.UnitOfWork;
using DNDocs.Infrastructure.Utils;
using DNDocs.Resources;
using DNDocs.Shared.Configuration;
using DNDocs.Shared.Log;
using DNDocs.Web.Application;
using DNDocs.Web.Application.Authorization;
using DNDocs.Web.Application.RateLimit;
using DNDocs.Web.Application.Validation;
using System.Runtime.InteropServices;
using static DNDocs.Infrastructure.Utils.RawRobiniaInfrastructure;
using DNDocs.Docs.Api.Client;
using DNDocs.Docs.Api.Shared;
using Vinca.Utils;
using DNDocs.Job.Api.Client;
using Vinca.Http.Logs;

namespace DNDocs.Web
{
    public class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();

            if (builder.Environment.IsDevelopment()) builder.Logging.AddConsole();

            if (builder.Environment.EnvironmentName == "IntegrationTests")
            {
                builder.Configuration.AddJsonFile("appsettings.Development.json", optional: false);
                builder.Configuration.AddJsonFile("appsettings.IntegrationTests.json", optional: false);
            }

            SetupSettings(builder.Configuration);

            var robiniaSettings = new DNDocsSettings();
            builder.Configuration.GetSection("DNDocsSettings").Bind(robiniaSettings);

            // Add services to the container.
            RegisterWebApp(builder);
            RegisterDomain(builder.Services);
            StartupRobiniaApplication.AddRobiniaApplication(builder);

            var app = builder.Build();

            app.UseCors(c => c.WithOrigins(robiniaSettings.CorsAllowedOrigins)
                .AllowAnyMethod()
                .AllowCredentials()
                .AllowAnyHeader());


            var fho = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All,
            };

            fho.KnownNetworks.Clear();
            fho.KnownProxies.Clear();

            app.UseForwardedHeaders(fho);
            app.UseVHttpLogs();
            app.UseVHttpExceptions();

            app.UseResponseCaching();
            app.UseMiddleware<RateLimitMiddleware>();

            var cultures = new[] { "en-US" };
            var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(cultures[0])
                .AddSupportedCultures(cultures)
                .AddSupportedUICultures(cultures);

            app.UseRequestLocalization(localizationOptions);

            app.Use(async (context, next) =>
            {
                var token = context.Request.Cookies["Token"];

                if (!string.IsNullOrEmpty(token) &&
                    !context.Request.Headers.ContainsKey("Authorization"))
                {
                    context.Request.Headers.Add("Authorization", "Bearer " + token);
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
                // is thi needed on linux?
            }

            app.UseStaticFiles(new StaticFileOptions() { RequestPath = "/api" });

            DeploySetup(app);

            if (app.Environment.IsDevelopment()) DevDataSeedSetup(app.Services);

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

        private static void SetupSettings(ConfigurationManager configuration)
        {
            var sRobiniaSettings = nameof(DNDocsSettings);

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

            // ThrowStartupException(
            //     !rs.Strings.FSProjectUrlForSitemapIndex.Contains("{0}") ||
            //     !rs.Strings.FSProjectUrlForSitemapIndex.Contains("{1}")
            //     , "Strings.DSProjectUrlForSitemapIndex must contain '{0}' and '{1}' for string.format(), 0 - project url prefix, {1} relative docfx file path");
            // ThrowStartupException(!rs.Strings.FSProjectDocsIndexUrl.Contains("{0}"), "FSProjectDocsIndexUrl does not contain '{0}' - this will be used to inject projetc url prefix");
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

        private static void RegisterWebApp(WebApplicationBuilder builder)
        {
            IServiceCollection services = builder.Services;
            var dsettings = new DNDocsSettings();
            builder.Configuration.GetSection($"{nameof(DNDocsSettings)}").Bind(dsettings);

            services.AddHttpClient();
            services.Configure<CookiePolicyOptions>(opt =>
            {
                opt.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });

            services.AddVHttpLogs(c => c.MaxQueueSize = 10000);
            builder.Services.AddResponseCaching();
            services.Configure<DNDocsSettings>(builder.Configuration.GetSection($"{nameof(DNDocsSettings)}"));
            services.AddDJobClientFactory();
            services.AddScoped<RobiniaApiControllerActionFilter>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAuthorizationPolicyProvider, RobiniaAuthorizationPolicyProvider>();
            services.AddTransient<IAuthorizationHandler, RobiniaAuthorizationHandler>();
            services.AddScoped<IScopeContext, ScopeContext>();
            services.AddScoped<IWebUser, WebUser>();
            services.AddDDocsApiClient(o => { o.ApiKey = dsettings.DDocsApiKey; o.ServerUrl = dsettings.DDocsServerUrl; });
            services.AddVNugetRepositoryFacade();
            builder.Services.AddAutoMapper(typeof(Program).Assembly);
            builder.Services.AddSingleton<IRobiniaResources, RobiniaResources>();

            builder.Services.AddControllers()
                .AddDataAnnotationsLocalization(opt =>
                {
                    opt.DataAnnotationLocalizerProvider = (type, factory) =>
                    {
                        return factory.Create(typeof(DefaultResources));
                    };
                });

            builder.Services.AddLocalization(opt =>
            {
                opt.ResourcesPath = "Resources";
            });

            builder.Services.Configure<FormOptions>(opt =>
            {
                // 16 Megabytes limit for all forms in system
                opt.MultipartBodyLengthLimit = 16 * 1024 * 1024;
            });

            builder.Services.AddRobiniaInfrastructure(dsettings.OSPathInfrastructureDirectory);
            builder.Services.AddTransient<IActionContextAccessor, ActionContextAccessor>();

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

#if DEBUG
            // builder.Services.BuildServiceProvider(new ServiceProviderOptions() { ValidateScopes = true });
#endif

            builder.Services.Configure<ApiBehaviorOptions>(options
                => options.SuppressModelStateInvalidFilter = true);

            services.AddOptions<DNDocsSettings>()
                .Bind(builder.Configuration.GetSection($"{nameof(DNDocsSettings)}"));
        }

        private static void RegisterDomain(IServiceCollection services)
        {
            var domainAssembly = typeof(DNDocs.Domain.Entity.App.AppLog).Assembly;
            var domainAllTypes = domainAssembly.GetTypes();

            // Services

            var serviceNamespace = typeof(DNDocs.Domain.Service.IProjectManager).Namespace;
            var serviceImplNamespace = typeof(DNDocs.Domain.ServiceImpl.ProjectManager).Namespace;

            var serviceInterfaces = domainAllTypes.Where(t => t.Namespace == serviceNamespace).ToList();
            var serviceImpl = domainAllTypes.Where(t => t.Namespace == serviceImplNamespace);

            // not sure, there are more classes than created in project (automatically created by framework?)
            serviceImpl = serviceImpl.Where(impl => serviceInterfaces.Any(inter => inter.IsAssignableFrom(impl)));

            foreach (var serviceImplementation in serviceImpl)
            {
                var serviceInterface = serviceInterfaces.First(iterf => iterf.IsAssignableFrom(serviceImplementation));

                services.AddTransient(serviceInterface, serviceImplementation);
            }

            services.AddScoped<IAppUnitOfWork, AppUnitOfWork>();
            services.AddSingleton<IRateLimitService, RateLimitService>(c =>
            {
                var logger = c.GetService<ILogger<RateLimitHandlerFixedWindow>>();
                var handlers = new RateLimitHandler[]
                {
                     new RateLimitHandlerFixedWindow(RLP.Project, TimeSpan.FromMinutes(30), 20, RLStandardBoxId.ByHeaderAuthorization, logger),
                     new RateLimitHandlerFixedWindow(RLP.TryItCreateProject, TimeSpan.FromMinutes(30), 5, RLStandardBoxId.ByIp, logger),
                     new RateLimitHandlerFixedWindow(RLP.Login, TimeSpan.FromMinutes(15), 3, RLStandardBoxId.ByIp, logger)
                };

                return new RateLimitService(handlers);
            });

            // Repositories
            IList<DIType> repos;
            RawRobiniaInfrastructure.ScanDI(out repos);

            foreach (var repoDI in repos)
            {
                services.AddTransient(repoDI.InterfaceType, repoDI.ImplementationType);
            }
        }

        private static void DevDataSeedSetup(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                // var acs = scope.ServiceProvider.GetRequiredService<IAdminCommandService>();
                // var aqs = scope.ServiceProvider.GetRequiredService<IAdminQueryService>();
                // var allProjs = aqs.GetAllProjects().ToArray();
                // 
                // if (allProjs.Any()) return;


                for (int i = 0; i < 2; i++)
                {
                    char l = (char)((int)'a' + i);
                    // acs.RequestProject("DevSeedProject" + l,
                    //     "Lorem ipsum dolor sit amet, consectetur adipiscing elit." +
                    //     "Suspendisse sit amet massa turpis. In sem risus, congue sed neque vel, iaculis tristique elit." +
                    //     "Suspendisse in ultrices ligula, at pharetra eros." +
                    //     "Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; Fusce ultrices nisl et nisi placerat vestibulum",
                    //     "githuburl" + l,
                    //     "robiniaurl" + l,
                    //     new List<BlobDataInfoDto>(),
                    //     new List<BlobDataInfoDto>(),
                    //     null,
                    //     null,
                    //     -1);
                }

                Thread.Sleep(1000);
                // allProjs = aqs.GetAllProjects().ToArray();

                // foreach (var p in allProjs) acs.DeployProject(p.Id, null);
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