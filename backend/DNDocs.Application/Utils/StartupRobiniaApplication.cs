using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using DNDocs.Shared.Log;
using DNDocs.Application.Application;
using DNDocs.Application.Shared;
using DNDocs.Shared.Log;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging.Configuration;
using DNDocs.Domain.Utils;
using DNDocs.Application.Services;
using Vinca.BufferLogger;

namespace DNDocs.Application.Utils
{
    public class StartupRobiniaApplication
    {
        public static void AddRobiniaApplication(WebApplicationBuilder builder)
        {
            IServiceCollection serviceCollection = builder.Services;
            AddDatabaseStoreLogger(builder.Logging);

            serviceCollection.AddVBufferLogger(c => c.MaxLogsTreshold = 10000);
            serviceCollection.AddHostedService<ApiBackgroundWorker>();
            serviceCollection.AddSingleton<ApiBackgroundWorker>();

            serviceCollection.AddScoped<ICommandDispatcher, CommandDispatcher>();
            serviceCollection.AddScoped<IQueryDispatcher, QueryDispatcher>();
            serviceCollection.AddScoped(typeof(ILog<>), typeof(Logg<>));
            serviceCollection.AddSingleton<IBgJobQueue, BgJobQueue>();

            var allCommandAndQueryHandlers = ReflectionFindAllHandlers();

            foreach (var type in allCommandAndQueryHandlers) serviceCollection.AddScoped(type);
        }

        private static ILoggingBuilder AddDatabaseStoreLogger(ILoggingBuilder builder)
        {
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, DatabaseStoreLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions
                <DatabaseStoreLoggerOptions, DatabaseStoreLoggerProvider>(builder.Services);


            return builder;
        }

        internal static Type[] ReflectionFindAllHandlers()
        {
            var allCommandAndQueryHandlers = typeof(StartupRobiniaApplication).Assembly.GetTypes()
                .Where(t =>
                {
                    Type basetype = t.BaseType;

                    if (basetype == null || !basetype.IsGenericType) return false;

                    Type baseGenericDef = basetype.GetGenericTypeDefinition();

                    bool result =
                        baseGenericDef == typeof(CommandHandler<>) ||
                        baseGenericDef == typeof(CommandHandler<,>) ||
                        baseGenericDef == typeof(CommandHandlerA<>) ||
                        baseGenericDef == typeof(CommandHandlerA<,>) ||
                        baseGenericDef == typeof(QueryHandler<,>) ||
                        baseGenericDef == typeof(QueryHandlerA<,>);

                    return result;
                })
                .ToArray();

            return allCommandAndQueryHandlers;
        }

        static Dictionary<Type, Type> cqHandlers = null;
        static object _lock = new object();

        internal static Type GetHandlerType(object commandOrQuery)
        {
            if (cqHandlers == null)
            {
                lock (_lock)
                {
                    if (cqHandlers == null)
                    {
                        var foundHandlers = new Dictionary<Type, Type>();

                        var all = StartupRobiniaApplication.ReflectionFindAllHandlers();

                        foreach (var handler in all)
                        {
                            var commandOrQueryType = handler.BaseType.GetGenericArguments()[0];

                            if (foundHandlers.ContainsKey(commandOrQueryType))
                                throw new RobiniaException("More than 1 handler found for specific command/query");

                            foundHandlers[commandOrQueryType] = handler;
                        }

                        cqHandlers = foundHandlers;
                    }
                }
            }

            if (!cqHandlers.ContainsKey(commandOrQuery.GetType()))
            {
                throw new RobiniaException($"Handler not found for type '{commandOrQuery.GetType().FullName}'");
            }

            var handlerType = cqHandlers[commandOrQuery.GetType()];

            return handlerType;
        }

        internal static object GetHandlerInstance(object commandOrQuery, IServiceProvider serviceProvider)
        {
            var handlerType = GetHandlerType(commandOrQuery);
            var handlerInstance = serviceProvider.GetService(handlerType);

            if (handlerInstance == null)
                throw new RobiniaException("Failed to get instance of handler from DI");

            return handlerInstance;
        }
    }
}
