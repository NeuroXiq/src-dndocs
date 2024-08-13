using Microsoft.Extensions.Logging;
using DNDocs.Shared.Log;
using DNDocs.Domain.Utils;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Infrastructure.DataContext;
using DNDocs.Shared.Utils;
using Newtonsoft.Json;

namespace DNDocs.Application.Shared
{
    public class HandlerData
    {
        public IServiceProvider scopedServiceProvider;
        public CancellationToken cancellationToken;
    }

    internal abstract class HandlerBase<TCommand, TResult>
    {
        protected ILogger logger;
        protected bool success = false;
        protected IAppUnitOfWork uow;

        private HandlerData handlerData;
        private bool isInit;
        protected CancellationToken cancellationToken;
        private AppDbContext appDbContext;
        string errorMessage = null;
        BusinessLogicException.FieldError[] fieldErrors = null;
        private TResult tresult = default(TResult);

        protected abstract Task<TResult> DoHandleAsync(TCommand command);

        public void Init(HandlerData data)
        {
            var prov = data.scopedServiceProvider.GetService(typeof(ILoggerProvider)) as ILoggerProvider;
            logger = prov.CreateLogger(this.GetType().FullName);
            handlerData = data;
            cancellationToken = data.cancellationToken;
            appDbContext = data.scopedServiceProvider.GetRequiredService<AppDbContext>();
            uow = data.scopedServiceProvider.GetRequiredService<IAppUnitOfWork>();
            isInit = true;
        }

        public async Task<CommandResult<TResult>> Run(TCommand command)
        {
            if (!isInit) throw new InvalidOperationException("call Init method because handler is not initialized");

            try
            {
                logger.LogTrace($"starting handler: {GetType().Name}");

                var start = DateTime.UtcNow;

                tresult = await DoHandleAsync(command);
                await appDbContext.SaveChangesAsync();

                var end = DateTime.UtcNow;

                logger.LogTrace($"completed handler: {GetType().Name}. Duration: {(end - start).ToString("c")}");

                success = true;
            }
            catch (Exception exc)
            {
                logger.LogTrace($"Failed to execute handler: {GetType().Name}, \r\ncommand: {JsonConvert.SerializeObject(command)}\r\nerror:\r\n{Helpers.ExceptionToStringForLogs(exc)}");

                Exception exception = exc;
                RobiniaException appExc = exc as RobiniaException;

                if (exc is AggregateException)
                {
                    exception = (exc as AggregateException).InnerException as RobiniaException;
                    appExc = exception as RobiniaException;
                }

                logger.LogWarning(exception, "handler exception");

                success = false;

                if (appExc != null)
                {
                    tresult = default(TResult);

                    if (exception is BusinessLogicException)
                    {
                        var e = exception as BusinessLogicException;
                        errorMessage = e.Error;
                        fieldErrors = e.FieldErrors?.ToArray() ?? new BusinessLogicException.FieldError[0];
                    }
                }
                else
                {
                    logger.LogError(exception, $"Unhandled exception\r\nCommand:\r\n{CommandDispatcher.SerializeCQException(command)}\r\n");

                    throw;
                }
            }

            return new CommandResult<TResult>(tresult, success, errorMessage, fieldErrors);
        }
    }

    internal abstract class CommandHandler<TCommand> : HandlerBase<TCommand, object>
    {
        public abstract void Handle(TCommand command);

        protected sealed override Task<object> DoHandleAsync(TCommand command)
        {
            Handle(command);

            return Task.FromResult(null as object);
        }
    }

    internal abstract class CommandHandler<TCommand, TResult> : HandlerBase<TCommand, TResult>
    {
        public abstract TResult Handle(TCommand command);

        protected sealed override Task<TResult> DoHandleAsync(TCommand command)
        {
            var r = Handle(command);

            return Task.FromResult(r);
        }
    }

    internal abstract class CommandHandlerA<TCommand, TResult> : HandlerBase<TCommand, TResult>
    {
        public abstract Task<TResult> Handle(TCommand command);

        protected override async Task<TResult> DoHandleAsync(TCommand command)
        {
            return await Handle(command);
        }
    }

    internal abstract class CommandHandlerA<TCommand> : HandlerBase<TCommand, object>
    {
        public abstract Task Handle(TCommand command);

        protected override async Task<object> DoHandleAsync(TCommand command)
        {
            await Handle(command);

            return Task.FromResult(null as object);
        }
    }

    //internal abstract class CommandHandler<TCommand> : HandlerBase, ICommandHandler<TCommand> where TCommand: ICommand
    //{
    //    public abstract void Handle(TCommand command);

    //    public CommandResult Run(TCommand command, HandlerData data)
    //    {
    //        var prov = data.serviceProvider.GetService(typeof(ILoggerProvider)) as ILoggerProvider;

    //        base.logger = prov.CreateLogger(this.GetType().FullName);

    //        BaseRunAction(() => Handle(command), command);

    //        return new CommandResult(base.Success, base.Error);
    //    }
    //}

    //internal abstract class CommandHandler<TCommand, TResult> : HandlerBase, ICommandHandler<TCommand, TResult> where TCommand: ICommand<TResult>
    //{
    //    public abstract TResult Handle(TCommand command);

    //    private TResult result = default(TResult);

    //    public CommandResult<TResult> Run(TCommand command, HandlerData data)
    //    {
    //        var prov = data.serviceProvider.GetService(typeof(ILoggerProvider)) as ILoggerProvider;

    //        base.logger = prov.CreateLogger(this.GetType().FullName);

    //        BaseRunAction(() =>
    //        {
    //            this.result = Handle(command);
    //        },
    //        command);

    //        return new CommandResult<TResult>(base.Success, base.Error, this.result);
    //    }
    //}
}
