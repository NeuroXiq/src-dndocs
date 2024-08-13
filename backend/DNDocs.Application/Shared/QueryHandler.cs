using Microsoft.Extensions.Logging;
using DNDocs.Domain.Utils;
using DNDocs.Shared.Log;
using System.Diagnostics;

namespace DNDocs.Application.Shared
{
    internal abstract class QueryHandlerBase<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        protected HandlerData handlerData;
        private bool isInit;
        protected ILogger logger;

        public QueryHandlerBase()
        {
            isInit = false;
        }

        protected abstract Task<TResult> DoHandleAsync(TQuery query);

        public void Init(HandlerData data)
        {
            handlerData = data;
            isInit = true;
            this.logger = ((ILoggerProvider)data.scopedServiceProvider.GetService(typeof(ILoggerProvider))).CreateLogger(this.GetType().FullName);
        }

        public async Task<QueryResult<TResult>> Run(TQuery query)
        {
            if (!isInit) throw new InvalidOperationException("Call 'Init' first because handle is not initialized");

            try
            {
                var start = DateTime.UtcNow;
                logger.LogTrace($"starting, {start.ToString("O")}");
                var result = await DoHandleAsync(query);
                logger.LogTrace("completed in {0}ms", (DateTime.UtcNow - start).TotalMilliseconds);

                return new QueryResult<TResult>(result);
            }
            catch (Exception e)
            {
#if DEBUG
                Debugger.Break();
#endif

                var known = e as RobiniaException;

                if (known != null)
                {
                    logger.LogWarning(e, "query handler exception");
                    var result = new QueryResult<TResult>(default(TResult));

                    return result;
                }

                logger.LogError(e, "unhandler error in query handler");

                throw e;
            }
        }
    }

    internal abstract class QueryHandler<TQuery, TResult> : QueryHandlerBase<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        protected abstract TResult Handle(TQuery query);

        protected override sealed Task<TResult> DoHandleAsync(TQuery query)
        {
            var result = Handle(query);

            return Task.FromResult(result);
        }
    }

    internal abstract class QueryHandlerA<TQuery, TResult> : QueryHandlerBase<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        protected abstract Task<TResult> Handle(TQuery query);

        protected override sealed async Task<TResult> DoHandleAsync(TQuery query)
        {
            var result = await Handle(query);

            return result;
        }   
    }
}
