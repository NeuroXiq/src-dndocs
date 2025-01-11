using DNDocs.IntergrationTests.Shared;

namespace DNDocs.IntegrationTests
{
    public class Tests2
    {
        [SetUp]
        public void Setup()
        {
            ITSetup.StartServer_DN();
            ITSetup.StartServer_DDocs();
            ITSetup.StartServer_DJob();
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}