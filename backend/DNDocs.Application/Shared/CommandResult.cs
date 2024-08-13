using DNDocs.Domain.Utils;

namespace DNDocs.Application.Shared
{
    public class CommandResult : HandlerResult
    {
        public CommandResult(bool success, string error, BusinessLogicException.FieldError[] fieldError) : base(success, error, fieldError)
        {
        }
    }

    public class CommandResult<TResult> : CommandResult
    {
        public CommandResult(TResult result, bool success, string error, BusinessLogicException.FieldError[] fieldError) : base(success, error, fieldError)
        {
            Result = result;
        }

        public TResult Result { get; set; }
    }
}
