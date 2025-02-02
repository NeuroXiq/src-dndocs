using DNDocs.Application.Queries;
using DNDocs.Application.Shared;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Api.DTO.Shared;

namespace DNDocs.Application.QueryHandlers
{
    internal class GetTableDataHandler : QueryHandler<GetTableDataQuery, TableDataDto<object>>
    {
        private ICurrentUser user;
        private IAppUnitOfWork uow;

        public GetTableDataHandler(
            ICurrentUser user,
            IAppUnitOfWork uow)
        {
            this.user = user;
            this.uow = uow;
        }

        protected override TableDataDto<object> Handle(GetTableDataQuery query)
        {
            user.AuthorizationAdmin();

            var logrepo = uow.AppLogRepository;
            var projsrepo = uow.ProjectRepository;

            DNDocs.Domain.ValueTypes.TableDataResponse r = null;

            if (query.TableDataRequest.TableName == "project")
            {
                r = projsrepo.GetTableData(query.TableDataRequest);
            }
            else if (query.TableDataRequest.TableName == "app_log")
            {
                r = logrepo.GetTableData(query.TableDataRequest);
            }
            else throw new Exception();

            return new TableDataDto<object>(r.RowsCount, r.CurrentPage, r.RowsPerPage, r.Data);
        }
    }
}
