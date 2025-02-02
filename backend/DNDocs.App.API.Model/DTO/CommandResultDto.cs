using DNDocs.Api.DTO.Enum;

namespace DNDocs.Api.DTO
{
    public class CommandResultDto : HandlerResultDto
    {
        public CommandResultDto() { }
        public CommandResultDto(bool success, string errorMessage, IEnumerable<FieldErrorDto> fieldErrors) : base(success, errorMessage, fieldErrors)
        {
        }
    }

    public class CommandResultDto<TResult> : HandlerResultDto
    {
        public TResult Result { get; set; }

        public CommandResultDto() { }

        public CommandResultDto(TResult result, bool success, string errorMessage, IEnumerable<FieldErrorDto> fieldErrors)
            : base(success, errorMessage, fieldErrors)
        {
            Result = result;
        }
    }

    public class FieldErrorDto
    {
        public string FieldName { get; set; }
        public string ErrorMessage { get; set; }

        public FieldErrorDto() { }

        public FieldErrorDto(string field, string error)
        {
            FieldName = field;
            ErrorMessage = error;
        }
    }
}
