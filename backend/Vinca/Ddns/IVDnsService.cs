namespace Vinca.Ddns
{
    public interface IVDdnsService
    {
        Task UpdateDdns(CancellationToken token);
    }
}
