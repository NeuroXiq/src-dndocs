namespace DNDocs.Application.Shared
{
    interface ICommandBgJob
    {
    }

    internal class CommandBgJob<TResult> : Command<TResult>, ICommandBgJob
    {
    }
}
