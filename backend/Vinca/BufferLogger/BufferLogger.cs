
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace Vinca.BufferLogger
{
    public sealed class BufferLogger : ILogger
    {
        //LoggerExternalScopeProvider

        // LoggerExternalScopeProvider
        // https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Logging.Abstractions/src/LoggerExternalScopeProvider.cs

        private static readonly ObjectPool<StringBuilder> stringBuilderPool = new DefaultObjectPoolProvider().CreateStringBuilderPool();

        Func<BufferLoggerOptions> getOptions;
        private string categoryName;
        private Action<LogRow> saveLogCallback;

        public BufferLogger(string name,
            Func<BufferLoggerOptions> getOptions,
            Action<LogRow> saveLogCallback
            )
        {
            this.categoryName = name;
            this.getOptions = getOptions;
            this.saveLogCallback = saveLogCallback;
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
                message += "\r\n" + ExceptionToStringForLogs(exception);

            var log = new LogRow()
            {
                CategoryName = this.categoryName,
                EventId = eventId,
                Date = DateTime.UtcNow,
                Message = message,
                LogLevel = logLevel
            };

            saveLogCallback(log);
        }

        string ExceptionToStringForLogs(Exception e)
        {
            if (e == null) return string.Empty;

            var b = stringBuilderPool.Get();
            b.AppendLine($"Type: {e.GetType().FullName}");
            b.AppendLine($"Message: {e.Message ?? ""}");
            b.AppendLine($"Source: {e.Source ?? ""}");
            b.AppendLine($"StackTrace:");
            b.AppendLine($"{e.StackTrace ?? ""}");

            if (e.InnerException != null)
            {
                b.AppendLine("InnerException:\r\n");
                b.Append(ExceptionToStringForLogs(e.InnerException));
            }

            return b.ToString();
        }
    }
}
