using DNDocs.Application.Shared;
using DNDocs.Api.DTO.Admin;
using DNDocs.Api.DTO.Shared;

namespace DNDocs.Application.Queries
{
    public class GetTableDataQuery : Query<TableDataDto<object>>
    {
        public TableDataRequest TableDataRequest { get; set; }
        
        public GetTableDataQuery(TableDataRequest request)
        {
            TableDataRequest = request;
        }
    }
}
