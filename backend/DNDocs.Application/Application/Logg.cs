using Microsoft.Extensions.Logging;
using DNDocs.Shared.Log;
using DNDocs.Shared.Log;

using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Utils;
using DNDocs.Infrastructure.Utils;
using DNDocs.Shared.Log;
using DNDocs.Shared.Utils;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DNDocs.Application.Application
{
    /// <summary>
    /// This class should be used everywhere when possible
    /// because it will store trace id. If this is impossible to use
    /// this class in some cases (e.g. in singleton services,  like BackgroundWorker) just use default 'Microsoft.extensions.ILogger<T> implemnetation.
    /// Scoped ILog can be useful e.g. access to userid, traceid etc.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Logg<T> : ILog<T>
    {
        private IScopeContext bridge;
        private string categoryName;
        private string TraceIdAlt = Guid.NewGuid().ToString().ToUpper();

        public Logg(IScopeContext bridge)
        {
            this.bridge = bridge;
            this.categoryName = typeof(T).FullName;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return default;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void MTrace(string msg, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int line = 0)
        {
            string logVal = $"{memberName}:{line}\r\n{filePath}\r\n{msg}";

            Log<object>(LogLevel.Trace, default(EventId), null, null, (s, e) => null);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string message = formatter(state, exception);
            if (exception != null)
                message += "\r\n" + Helpers.ExceptionToStringForLogs(exception);

            // if there is no HTTP context trace id just use unique guid of current
            // instance, because this service must be registered as scoped, so each scope generates unique id
            var traceId = bridge.TraceIdentifier ?? TraceIdAlt;

            var log = new AppLog(
                message,
                categoryName,
                (int)logLevel,
                eventId.Id,
                eventId.Name,
                DateTime.UtcNow,
                traceId
                );

            // ApiBackgroundWorker.Log(log);
        }
    }
}
