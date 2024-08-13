using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Service;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Domain.ValueTypes;
using DNDocs.Infrastructure.DomainServices;
using DNDocs.Shared.Configuration;
using System.IO.Compression;

namespace DNDocs.Infrastructure.Utils
{
    internal class AppManager : IAppManager, IDisposable
    {
        private IAppUnitOfWork appuow;
        private IScopeContext bridge;
        private IServiceScopeFactory serviceScopeFactory;
        private DNDocsSettings rsettings;
        private IServiceProvider serviceProvider;
        private List<IGit> gitInstances = new List<IGit>();

        //private IActionContextAccessor aca;
        private ICurrentUser currentUser;
        private IDNInfrastructure robiniaInfrastructure;

        public AppManager(IDNInfrastructure robiniaInfrastructure,
            //IActionContextAccessor aca,
            IScopeContext bridge,
            IServiceScopeFactory serviceScopeFactory,
            IAppUnitOfWork appuow,
            ICurrentUser currentUser,
            IAppUnitOfWork appUow,
            IServiceProvider serviceProvider,
            IOptions<DNDocsSettings> robiniaSettings)
        {
            this.rsettings = robiniaSettings.Value;
            this.serviceProvider = serviceProvider;
            this.currentUser = currentUser;
            this.appuow = appuow;
            this.bridge = bridge;
            // this.httpContextAccessor = accessor;
            this.serviceScopeFactory = serviceScopeFactory;
            //this.aca = aca;
            this.robiniaInfrastructure = robiniaInfrastructure;
        }

        public ExecuteRawSqlResult ExecuteRawSql(string dbRelativePath, int mode, string sqlcode)
        {
            var connectionString = RawRobiniaInfrastructure.ConnectionStringForFile(dbRelativePath);

            var result = new ExecuteRawSqlResult();
            result.Success = false;
            result.ExecuteMode = mode;

            try
            {
                using (var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqliteCommand(sqlcode, connection);

                    if (mode == 1)
                    {
                        var reader = command.ExecuteReader();
                        var columnsCount = reader.FieldCount;
                        string[] columns = Enumerable.Range(0, columnsCount)
                            .Select(colIndex => reader.GetName(colIndex))
                            .ToArray();

                        List<object[]> results = new List<object[]>();

                        while (reader.Read())
                        {
                            object[] row = new object[columnsCount];

                            for (int i = 0; i < columnsCount; i++)
                            {
                                row[i] = reader.GetValue(i);

                                if (row[i] != null) row[i] = row[i].ToString();
                            }

                            results.Add(row);
                        }

                        result.Columns = columns;
                        result.Rows = results.ToArray();
                        result.ExecuteNonQueryResult = null;
                    }
                    else
                    {
                        result.Columns = null;
                        result.Rows = null;
                        result.ExecuteNonQueryResult = command.ExecuteNonQuery();
                    }

                    result.Success = true;
                }
            }
            catch (Exception e) { result.Exception = e.ToString(); }


            return result;
        }

        public string GetOSPathGitRepoStoreRepo(Guid uuid)
        {
            return robiniaInfrastructure.GetOSPathGitRepoStoreRepo(uuid);
        }

        public IGit OpenGitRepo(string repoUrl)
        {
            // var gitInstance = new Git(repoUrl,
            //     rsettings.GitExeFilePath,
            //     serviceProvider.GetRequiredService<ISystemProcess>(),
            //     serviceProvider.GetRequiredService<IAppUnitOfWork>(),
            //     serviceProvider,
            //     serviceProvider.GetRequiredService<IAppManager>());

            var git = serviceProvider.GetRequiredService<IGit>();
            git.InitInstance(repoUrl);

            gitInstances.Add(git);

            return git;
        }

        public void Dispose()
        {
            // no better idea how to dispose this, because there are created in this class
            gitInstances.ForEach(a => a.Dispose());
        }

        public bool GitRepoExistsURL(string repoUrl)
        {
            var git = serviceProvider.GetRequiredService<IGit>();
            return git.RepositoryExistsURL(repoUrl);
        }
    }
}
