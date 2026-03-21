using DNDocs.Domain.Utils;

namespace DNDocs.Application.Shared
{
    public class CommandResult : HandlerResult
    {
        public CommandResult()
        {
        }
    }

    public class CommandResult<TResult> : CommandResult
    {
        public CommandResult(TResult result)
        {
            Result = result;
        }

        public TResult Result { get; set; }
    }
}
