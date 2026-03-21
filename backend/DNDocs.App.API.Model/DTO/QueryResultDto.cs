using DNDocs.Api.DTO.Enum;

namespace DNDocs.Api.DTO
{
    public class QueryResultDto<TResult> : HandlerResultDto
    {
        public QueryResultDto()
        {
        }

        public TResult Result { get; set; }
    }
}
