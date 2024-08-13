using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Docs.IntegrationTests.Shared
{
    public class TestsAppConfig
    {
        // copy-paste from appsettings.development dndocs.docs
        public static string ApiKey = "@T4hjr4dsh$%H$J%45j6t7kY^zsdg34";
        public static string DdocsHttpsUrl = "https://127.0.0.1:7088";
        public static string PathDndocsDocsCsproj = "C:\\my-files\\projects\\DNDocsRepo\\backend\\DNDocs.Docs.Web\\DNDocs.Docs.Web.csproj";
        public static string PathSmallSizeZip = "C:\\my-files\\projects\\DNDocsRepo\\var\\small-site-sqlite.zip";
        public static string PathBigSiteZip = @"C:\my-files\projects\DNDocsRepo\var\big-site-ef.zip";
        public static string PathSuperSmallSiteZip = @"C:\my-files\projects\DNDocsRepo\var\super-small-site.zip";
        public static string PathDDocsTestsInfrastructureDir = @"C:\my-files\projects\DNDocsRepo\var\it-ddocs";
        
    }
}
