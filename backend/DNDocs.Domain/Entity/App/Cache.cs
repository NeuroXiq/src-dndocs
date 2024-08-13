using DNDocs.Domain.Entity.Shared;
using DNDocs.Domain.Utils;

namespace DNDocs.Domain.Entity.App
{
    public class Cache : Entity, ICreateUpdateTimestamp
    {
        public DateTime CreatedOn { get; set; }
        public DateTime LastModifiedOn { get; set; }

        public DateTime Expiration { get; set; }
        public string Key { get; set; }
        public byte[] Data { get; set; }

        public Cache() { }

        public Cache(string key, byte[] data, DateTime exp)
        {
            Validation.AppEx(string.IsNullOrWhiteSpace(key), "empty key");
            Validation.AppEx(exp < DateTime.UtcNow, "exp is less than current datetime");

            Key = key;
            Data = data;
            Expiration = exp;
        }

        public void Update(byte[] data, DateTime exp)
        {
            Data = data;
            Expiration = exp;
        }
    }
}
