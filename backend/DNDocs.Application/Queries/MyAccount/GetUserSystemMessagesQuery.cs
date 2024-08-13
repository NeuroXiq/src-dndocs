using DNDocs.Application.Shared;
using DNDocs.Api.DTO.Shared;

namespace DNDocs.Application.Queries.MyAccount
{
    public class GetUserSystemMessagesQuery : Query<TableDataDto<SystemMessageDto>>
    {
        public int? ProjectId { get; set; }
        public int? ProjectVersioningId { get; set; }
        public int PageNo { get; set; }
        public int RowsPerPage { get; set; }

        public GetUserSystemMessagesQuery()
        { }

        public GetUserSystemMessagesQuery(int pageNo, int rowsPerPage, int? projectId, int? projectVersioningId)
        {
            ProjectVersioningId = projectVersioningId;
            ProjectId = projectId;
            PageNo = pageNo;
            RowsPerPage = rowsPerPage;
        }
    }
}
