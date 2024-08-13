using DNDocs.Application.Queries.Admin;
using DNDocs.Application.Shared;
using DNDocs.Domain.Utils;
using DNDocs.Api.DTO.Admin;

namespace DNDocs.Application.QueryHandlers.Admin
{
    internal class ExecuteRawSqlHandler : QueryHandler<ExecuteRawSqlQuery, ExecRawSqlResultDto>
    {
        private ICurrentUser user;
        private IAppManager appManager;

        public ExecuteRawSqlHandler(
            IAppManager appManager,
            ICurrentUser user)
        {
            this.user = user;
            this.appManager = appManager;
        }

        protected override ExecRawSqlResultDto Handle(ExecuteRawSqlQuery query)
        {
            var result = appManager.ExecuteRawSql(query.DbName, (int)query.Mode, query.SqlCode);

            return new ExecRawSqlResultDto
            {
                Columns = result.Columns,
                Rows = result.Rows,
                ExecuteMode = (DNDocs.Api.DTO.Enum.RawSqlExecuteMode)result.ExecuteMode,
                ExecuteNonQueryResult = result.ExecuteNonQueryResult,
                Success = result.Success,
                Exception = result.Exception
            };
        }
    }
}
