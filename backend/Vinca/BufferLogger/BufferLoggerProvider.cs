using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Runtime.Versioning;

namespace Vinca.BufferLogger
{
    public interface IVBufferLogger
    {
        bool IsEmpty { get; }
        IList<LogRow> DequeueAllLogs();
        void SetCallbackOnMaxQueuedLogs(Action action);
    }

    [UnsupportedOSPlatform("browser")]
    [ProviderAlias("BufferLogger")]
    public sealed class BufferLoggerProvider : ILoggerProvider, IVBufferLogger
    {
        public bool IsEmpty { get { return this.logs.IsEmpty; } }

        private BufferLoggerOptions options;
        private IDisposable onChangeToken;
        private readonly IServiceProvider serviceProvider;
        private readonly ConcurrentDictionary<string, BufferLogger> loggers;

        private object _lock = new object();
        private ConcurrentQueue<LogRow> logs;
        private Action onMaxQueuedLogs;

        public BufferLoggerProvider(
            IServiceProvider serviceProvider,
            IOptionsMonitor<BufferLoggerOptions> options)
        {
            this.serviceProvider = serviceProvider;
            this.options = options.CurrentValue;
            onChangeToken = options.OnChange(updated => this.options = updated);
            loggers = new ConcurrentDictionary<string, BufferLogger>();
            logs = new ConcurrentQueue<LogRow>();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return loggers.GetOrAdd(categoryName, name => new BufferLogger(name, GetCurrentOptions, AppendLog));
        }

        BufferLoggerOptions GetCurrentOptions() => options;

        public void SetCallbackOnMaxQueuedLogs(Action callback)
        {
            onMaxQueuedLogs = callback;
        }

        public void Dispose()
        {
            loggers.Clear();
            onChangeToken?.Dispose();
        }

        // todo object pool for logs, and pass all data as method parameter?
        private void AppendLog(LogRow row)
        {
            logs.Enqueue(row);

            if (logs.Count > options.MaxLogsTreshold && onMaxQueuedLogs != null) onMaxQueuedLogs();
        }

        public IList<LogRow> DequeueAllLogs()
        {
            var nowCount = logs.Count;
            var result = new List<LogRow>();

            if (nowCount == 0) return result;

            for (int i = 0; i < nowCount && logs.TryDequeue(out var logRow); i++) result.Add(logRow);

            return result;
        }
    }

    public static class TickLoggerExtensions
    {
        public static void AddVBufferLogger(this IServiceCollection services, Action<BufferLoggerOptions> configue)
        {
            services.Configure(configue);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, BufferLoggerProvider>());
            services.AddSingleton<IVBufferLogger>(x => (IVBufferLogger)x.GetServices<ILoggerProvider>().Where(t => t is BufferLoggerProvider).First());
        }
    }
}
