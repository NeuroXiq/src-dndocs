using System.Net.Http.Json;

namespace DNDocs.IntegrationTests
{
    [TestFixture]
    public class DNDocsHomeTests : IntegrationTestsBase
    {
        [Test]
        public async Task Index_HttpGet_Ok()
        {
            var result = await DNDocsHttpClient.GetStringAsync("/");
            Assert.That(result?.Length > 0, "empty response");
        }

        [Test]
        public async Task Index_Generate_HttpGet_Ok()
        {
            var result = await DNDocsHttpClient.GetStringAsync("/generate/Arctium/1.0.11");

            Assert.That(result?.Length > 0, "empty content");
            Assert.That(result.Contains("Arctium"), "no 'arctium' string in result");
            Assert.That(result.Contains("1.0.11"), "no '1.0.11' version string in result");
        }

        [Test]
        public async Task FullCreateNugetProject_Success()
        {
            // var result = await DNDocsHttpClient.PostAsJsonAsync("/api/", );
        }

        [Test]
        public void CreateNugetProject_FailOnDuplicated() { throw new Exception(); }

        [Test]
        public void WillCreateMultipleParallelProjects() { throw new Exception(); }

        [Test]
        public void WillReturnImmediatelyIfProjectAlreadyExists() { throw new Exception(); }
    }
}
