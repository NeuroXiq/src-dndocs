using DNDocs.Api.DTO.Enum;

namespace DNDocs.Api.DTO
{
    public class CommandResultDto : HandlerResultDto
    {
        public CommandResultDto() { }
    }

    public class CommandResultDto<TResult> : HandlerResultDto
    {
        public TResult Result { get; set; }

        public CommandResultDto() { }

        public CommandResultDto(TResult result)
        {
            Result = result;
        }
    }
}
