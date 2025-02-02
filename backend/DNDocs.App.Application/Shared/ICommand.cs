namespace DNDocs.Application.Shared
{
    public interface ICommand { }

    public interface ICommand<TResult> : ICommand
    {
    }
}
