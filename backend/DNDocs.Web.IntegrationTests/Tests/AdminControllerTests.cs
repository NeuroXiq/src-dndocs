using DNDocs.Web.Controllers;
using DNDocs.Api.Client;
using DNDocs.Api.DTO.Admin;
using DNDocs.Api.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.IntegrationTests.Shared;

namespace Web.IntegrationTests.Tests
{
    public class AdminControllerTests : TestsBase
    {
        public AdminControllerTests()
        {
            base.AuthType = AuthUserType.Admin;
        }

        protected override AuthUserType DefaultAuthUser() => AuthUserType.Admin;

        [Test]
        public void GetDashboardInfo_NotAuthorizedWithoutToken()
        {
            // arrange
            AuthType = AuthUserType.NoAuth;

            // act
            var result = RawApiCall(new HttpRequestMessage(HttpMethod.Get, Urls.Admin.GetDashboardInfo));

            // assert
            Assert.That(result.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        }


        [Test]
        public void GetDashboardInfo_NotAuthorizedIfNotAdmin()
        {
            // arrange
            AuthType = AuthUserType.TestUser1;

            // act
            var result = RawApiCall(new HttpRequestMessage(HttpMethod.Get, Urls.Admin.GetDashboardInfo));

            // assert
            Assert.That(result.StatusCode == System.Net.HttpStatusCode.Forbidden);
        }

        [Test]
        public void GetDashboardInfo_ReturnsSuccessAndNotNullObject()
        {
            // arrange & act
            var result = base.HttpGetQ<AdminDashboardInfoDto>(Urls.Admin.GetDashboardInfo);

            // assert
            Assert.NotNull(result);
            Assert.NotNull(result.Result);
            Assert.Zero(result.Result.Projects.Data.Count());
        }

        [Test]
        public void ExecuteRawSql_ReturnsSuccessOnValidRequest()
        {
            // arrange
            string query = "insert into sitemap_file" +
                "(created_on, last_modified_on, file_name, file_content) " +
                $"values" + 
                $"('{DateTime.Now.ToString("o")}', '{DateTime.Now.ToString("o")}', " +
                "'integration_test_file', 'integration_test_file')";

            // act
            var r = HttpPostR<ExecRawSqlResultDto>(Urls.Admin.ExecuteRawSql,
                new ExecuteRawSqlModel {
                    Mode = DNDocs.Api.DTO.Enum.RawSqlExecuteMode.ExecuteNonQuery,
                    DbName = "app/appdb.sqlite", 
                    SqlCode = query });

            var queryResult = HttpPostR<ExecRawSqlResultDto>(Urls.Admin.ExecuteRawSql,
                new ExecuteRawSqlModel()
                {
                    DbName = "app/appdb.sqlite",
                    Mode = DNDocs.Api.DTO.Enum.RawSqlExecuteMode.ExecuteReader,
                    SqlCode = "SELECT * FROM sitemap_file WHERE file_name = 'integration_test_file'"
                });

            // assert
            Assert.True(r.Success);
            Assert.That(queryResult.Rows.Count() == 1);
        }
    }
}
