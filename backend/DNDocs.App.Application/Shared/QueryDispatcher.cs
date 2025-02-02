using DNDocs.Application.Utils;

namespace DNDocs.Application.Shared
{
    public interface IQueryDispatcher
    {
        QueryResult<TResult> DispatchSync<TResult>(IQuery<TResult> query);
        Task<QueryResult<TResult>> DispatchAsync<TResult>(IQuery<TResult> query);
    }

    internal class QueryDispatcher : IQueryDispatcher
    {
        private IServiceProvider serviceProvider;

        public QueryDispatcher(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public QueryResult<TResult> DispatchSync<TResult>(IQuery<TResult> query)
        {
            var task = InvokeHandler(query);

            return task.Result;
        }

        public async Task<QueryResult<TResult>> DispatchAsync<TResult>(IQuery<TResult> query)
        {
            var task = InvokeHandler(query);
            
            return await task;
        }

        private Task<QueryResult<TResult>> InvokeHandler<TResult>(IQuery<TResult> query)
        {
            var handlerObj = StartupRobiniaApplication.GetHandlerInstance(query, serviceProvider);
            var handlerData = new HandlerData() { scopedServiceProvider = this.serviceProvider };

            handlerObj.GetType().GetMethod("Init").Invoke(handlerObj, new object[] { handlerData });
            var result = handlerObj.GetType().GetMethod("Run").Invoke(handlerObj, new object[] { query }) as Task<QueryResult<TResult>>;

            return result;
        }
    }
}
