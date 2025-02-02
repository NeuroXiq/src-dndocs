using DNDocs.Domain.Service;
using DNDocs.Domain.ValueTypes;

namespace DNDocs.Domain.Utils
{
    public interface IAppManager
    {
        ExecuteRawSqlResult ExecuteRawSql(string dbname, int mode, string sqlcode);
        string GetOSPathGitRepoStoreRepo(Guid uuid);
        bool GitRepoExistsURL(string repoUrl);
    }
}
