namespace Vinca.Ddns
{
    public interface IVDdnsService
    {
        Task UpdateDdnsAsync(CancellationToken token);
    }
}
