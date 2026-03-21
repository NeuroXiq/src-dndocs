namespace DNDocs.Application.Shared
{
    public class QueryResult<TResult> : HandlerResult
    {
        public TResult Result { get; set; }

        public QueryResult(TResult result)
        {
            Result = result;
        }
    }
}
