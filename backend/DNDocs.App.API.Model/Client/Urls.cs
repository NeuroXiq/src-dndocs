namespace DNDocs.Api.Client
{
    public class Urls
    {
        public const string Integration_DJobBuildCompleted = "api/integration/djobbuildcompleted";
        public const string Integration_DJobRegisterService = "api/integration/djobregisterservice";

        public static HomeRoutes Home = new HomeRoutes("/api/home");
        public static AuthRoutes Auth = new AuthRoutes("/api/auth");
        public static AdminRoutes Admin = new AdminRoutes("/api/admin");
        public static ProjectManageRoutes Project = new ProjectManageRoutes("/api/projectmanage");
        public static MyAccountRoutes MyAccount = new MyAccountRoutes("/api/myaccount");
        public static OtherRoutes Other = new OtherRoutes("/api/other");

        public class RoutesBase
        {
            protected string p;

            public RoutesBase(string prefix) { p = prefix; }

            protected string R(string action) => $"{p}/{action}";
        }

        public class OtherRoutes : RoutesBase
        {
            public readonly string Sitemaps;

            public OtherRoutes(string p) : base(p)
            {
                Sitemaps = R("Sitemaps");
            }

        }

        public class MyAccountRoutes : RoutesBase
        {
            public readonly string GetMyProjects;
            public readonly string GetAccountDetails;
            public readonly string GetSystemMessages;
            public readonly string WaitingForBgJob;

            public MyAccountRoutes(string p) : base(p)
            {
                GetMyProjects = R("GetMyProjects");
                GetAccountDetails = R("GetAccountDetails");
                GetSystemMessages = R("GetSystemMessages");
                WaitingForBgJob = R("WaitingForBgJob");
            }
        }

        public class ProjectManageRoutes : RoutesBase
        {
            public readonly string DeleteProject;
            public readonly string GetProjectById;
            public readonly string RequestAutoupgrade;
            public readonly string RequestProject;
            public readonly string CreateProjectVersioning;
            public readonly string CreateProjectVersion;
            public readonly string CreateProjectVersionByGitTag;

            public readonly string GetAllProjectVersioning;
            public readonly string GetProjectVersioningById;
            public readonly string GetProjectsVersioningInfo;
            public readonly string GetVersioningGitTags;

            public string FormatGetProjectVersioningById(int versioningid) => $"{GetProjectVersioningById}?id={versioningid}";

            public ProjectManageRoutes(string p) : base(p)
            {
                DeleteProject = R("DeleteProject");
                GetProjectById = R("GetProjectById");
                RequestAutoupgrade = R("RequestAutoupgrade");
                RequestProject = R("RequestProject");
                CreateProjectVersioning = R("CreateProjectVersioning");
                CreateProjectVersion = R("CreateProjectVersion");
                CreateProjectVersionByGitTag = R("CreateProjectVersionByGitTag");

                GetAllProjectVersioning = R("GetAllProjectVersioning");
                GetProjectVersioningById = R("GetProjectVersioningById");
                GetProjectsVersioningInfo = R("GetProjectsVersioningInfo");
                GetVersioningGitTags = R("GetVersioningGitTags");

            }
        }

        public class AuthRoutes : RoutesBase
        {
            public readonly string AdminLogin;
            public readonly string CallbackGithubOAuth;

            public AuthRoutes(string prefix) : base(prefix)
            {
                AdminLogin = R("AdminLogin");
                CallbackGithubOAuth = R("CallbackGithubOAuth");
            }

            
        }

        public class AdminRoutes : RoutesBase
        {
            public readonly string GetDashboardInfo;
            public readonly string GetTableData;
            public readonly string DoBackgroundWorkNow;
            public readonly string ExecuteRawSql;

            public AdminRoutes(string prefix) : base(prefix)
            {
                GetDashboardInfo = R("GetDashboardInfo");
                GetTableData = R("GetTableData");
                DoBackgroundWorkNow = R("DoBackgroundWorkNow");
                ExecuteRawSql = R("ExecuteRawSql");
            }
        }

        public class HomeRoutes : RoutesBase
        {
            public readonly string GetAllProjects;
            public readonly string GetVersionInfo;
            public readonly string TryItCreateProject;

            public HomeRoutes(string prefix) : base(prefix)
            {
                GetAllProjects = R("GetAllProjects");
                GetVersionInfo = R("GetVersionInfo");
                TryItCreateProject = R("TryItCreateProject");
            }
        }
    }
}
