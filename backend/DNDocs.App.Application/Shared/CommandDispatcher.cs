using DNDocs.Application.Utils;
using DNDocs.Domain.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DNDocs.Application.Shared
{
    public interface ICommandDispatcher
    {
        CommandResult Dispatch(ICommand command, CancellationToken cancellationToken = default);
        public CommandResult<TResult> Dispatch<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
        public Task<CommandResult> DispatchAsync(ICommand command, CancellationToken cancellationToken = default);
        public Task<CommandResult<TResult>> DispatchAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

        public object DispatchGeneric(ICommand command, CancellationToken cancellationToken = default);
    }

    internal class CommandDispatcher : ICommandDispatcher
    {
        private IServiceProvider serviceProvider;

        public CommandDispatcher(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public CommandResult Dispatch(ICommand command, CancellationToken ct = default)
        {
            return SyncTask(DispatchGeneric<object>(command, ct));
        }

        public CommandResult<TResult> Dispatch<TResult>(ICommand<TResult> command, CancellationToken ct = default)
        {
            return SyncTask(DispatchGeneric<TResult>(command, ct));
        }

        private CommandResult<TResult> SyncTask<TResult>(Task<CommandResult<TResult>> t)
        {
            Task.WaitAny(t);
            
            if (t.Exception != null)
            {
                // re-throw exception because original will throw 'AggregateException'
                // and we want like 'RobinaException' etc.
                throw t.Exception.InnerException;
            }

            return t.Result;
        }

        public async Task<CommandResult> DispatchAsync(ICommand command, CancellationToken ct = default)
        {
            return await DispatchGeneric<object>(command, ct);
        }

        public async Task<CommandResult<TResult>> DispatchAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default)
        {
            return await DispatchGeneric<TResult>(command, ct);
        }

        public object DispatchGeneric(ICommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var handlerData = new HandlerData
            {
                scopedServiceProvider = this.serviceProvider,
                cancellationToken = cancellationToken
            };

            var handler = StartupRobiniaApplication.GetHandlerInstance(command, serviceProvider);
            handler.GetType().GetMethod("Init").Invoke(handler, new object[] { handlerData });

            var task = handler.GetType().GetMethod("Run").Invoke(handler, new object[] { command });
            
            // var taskResultType = task?.GetType();
            //if (taskResultType.GetGenericTypeDefinition() != typeof(Task<>) ||
            //    taskResultType.GenericTypeArguments[0].GetGenericTypeDefinition() != typeof(CommandResult<>)
            //    )
            //{
            //    // for safety reasons for future changes

            //    throw new Exception(
            //        "Code in this class assumes task handler returns Task<CommandResult<TResult>> but result is other." +
            //        "If base command handler was modified modify rest of the code of this commandsipatched class to work correctly");
            //}

            return task;
        }

        private Task<CommandResult<TResult>> DispatchGeneric<TResult>(ICommand command, CancellationToken ct = default)
        {
            var orgResult = DispatchGeneric(command, ct);
            var result = orgResult as Task<CommandResult<TResult>>;

            if (result == null)
            {
                throw new Exception("invalid TResult value, cannot cast to Task<CommandResult<TResult>>. Does Command return type match CommandHandler return type?");
            }

            return result;
        }

        internal static string SerializeCQException(object commandOrQuery)
        {
            return JsonSerializer.Serialize(commandOrQuery, commandOrQuery.GetType());
        }
    }
}
