using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DNDocs.Shared.Log;
using DNDocs.Application.Application;
using DNDocs.Domain.Entity.App;
using DNDocs.Shared.Utils;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace DNDocs.Application.Application
{
    public sealed class DatabaseStoreLogger : ILogger
    {
        //LoggerExternalScopeProvider

        // LoggerExternalScopeProvider
        // https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Logging.Abstractions/src/LoggerExternalScopeProvider.cs
        private static readonly AsyncLocal<string> traceIdAsyncLocal = new AsyncLocal<string>();
        private static string TraceId
        {
            get
            {
                if (traceIdAsyncLocal.Value == null)
                {
                    traceIdAsyncLocal.Value = Guid.NewGuid().ToString().ToUpper();
                }

                return traceIdAsyncLocal.Value;
            }
        }


        Func<DatabaseStoreLoggerOptions> getOptions;
        private IServiceScopeFactory scopeFactory;
        private IHttpContextAccessor httpContextAccessor;
        private string categoryName;


        public DatabaseStoreLogger(string name,
            Func<DatabaseStoreLoggerOptions> getOptions,
            IServiceScopeFactory scopeFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.categoryName = name;
            this.getOptions = getOptions;
            this.scopeFactory = scopeFactory;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return default;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            string message = formatter(state, exception);
            if (exception != null)
                message += "\r\n" + Helpers.ExceptionToStringForLogs(exception);

            var log = new AppLog(
                message,
                categoryName,
                (int)logLevel,
                eventId.Id,
                eventId.Name,
                DateTime.UtcNow,
                TraceId
                );

            // for now no better idea (how to inject background-thread service?)
            // so use static class
            // ApiBackgroundWorker.Log(log);
        }
    }
}
