namespace DNDocs.IntegrationTests
{

    public class ManagementControllerNugetProject : TestsBase
    {
        //[Test]
        //public void Ping_WillThrowIfInvalidApiKey()
        //{
        //    // ignore this is not needed in local env
        //    var handler = new HttpClientHandler();
        //    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        //    handler.ServerCertificateCustomValidationCallback =
        //        (httpRequestMessage, cert, cetChain, policyErrors) =>
        //        {
        //            return true;
        //        };
        //    var client = new HttpClient(handler);

        //    client.DefaultRequestHeaders.Add("x-api-key", "invalid");
        //    var result = client.GetAsync($"{base.TestServerUrl}/{DUrls.Management_Ping}").Result;

        //    AssertStatusCode(result.StatusCode, System.Net.HttpStatusCode.Unauthorized);
        //}

        //[Test]
        //public async Task Ping_WillSucceessWithValidApiKey()
        //{
        //    var result = await Client.Management_Ping("test - ping");

        //    Assert.That(result == "test - ping");
        //}

        //[Test]
        //public async Task CreateProject_WillNotCreateNugetProjectWithSamePackage()
        //{
        //    var siteZip = base.GetSmallSiteFileStream();
        //    var samepkgname = NextPkgName;
        //    var samepkgver = NextPkgVer;


        //}

        //[Test]
        //public async Task CreateProject_WillCreateNugetProject()
        //{
        //    var siteZip = base.GetSmallSiteFileStream();


        //}

        //[Test]
        //public async Task CreateProject_WillNotCreateNugetProjectWithSamePackages()
        //{
        //    var sameNugetPackage = "same-nuget-pkb-1.2.3.4";


        //}

        //[Test]
        //public void CreateProject_Nuget_WillFailOnInvalidData()
        //{
        //    string[] s = new string[]
        //    {
        //        "", NextPkgVer, null,
        //        NextProjectName, "     ", null,
        //        NextProjectName, NextPkgVer, "prefix-invalid-for-nuget",
        //        null, NextPkgVer, null,
        //    };

        //    for (int i = 0; i < s.Length; i++)
        //    {
        //        Assert.That(() =>
        //        {
        //            string pname = s[(i * 3 + 0)];
        //            string pver = s[(i * 3 + 1)];
        //            string urlprefix = s[(i * 3 + 2)];


        //        }, Throws.Exception);
        //    }
        //}
    }
}
