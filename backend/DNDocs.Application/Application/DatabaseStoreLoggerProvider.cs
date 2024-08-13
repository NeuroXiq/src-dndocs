using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DNDocs.Shared.Log;
using Microsoft.Extensions.Options;
using DNDocs.Domain.Utils;
using System.Collections.Concurrent;
using System.Runtime.Versioning;

namespace DNDocs.Application.Application
{
    [UnsupportedOSPlatform("browser")]
    [ProviderAlias("DatabaseStore")]
    public sealed class DatabaseStoreLoggerProvider : ILoggerProvider
    {
        private IHttpContextAccessor httpContextAccessor;
        private DatabaseStoreLoggerOptions options;
        private IDisposable onChangeToken;
        private IServiceScopeFactory scopeFactory;
        private readonly IServiceProvider provider;
        private readonly ConcurrentDictionary<string, DatabaseStoreLogger> loggers;

        public DatabaseStoreLoggerProvider(
            IOptionsMonitor<DatabaseStoreLoggerOptions> options,
            IServiceScopeFactory scopeFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.options = options.CurrentValue;
            onChangeToken = options.OnChange(updated => this.options = updated);
            this.scopeFactory = scopeFactory;
            loggers = new ConcurrentDictionary<string, DatabaseStoreLogger>();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return loggers.GetOrAdd(categoryName, name => new DatabaseStoreLogger(name, GetCurrentOptions, scopeFactory, httpContextAccessor));
        }

        DatabaseStoreLoggerOptions GetCurrentOptions() => options;

        public void Dispose()
        {
            loggers.Clear();
            onChangeToken?.Dispose();
        }
    }
}
