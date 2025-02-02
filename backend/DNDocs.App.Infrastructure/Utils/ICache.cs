using DNDocs.Domain.Repository;
using DNDocs.Domain.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using DNDocs.Domain.Utils;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using System.Text;
using DNDocs.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace DNDocs.Infrastructure.Utils
{
    /// <summary>
    /// Implementation of two kinds of cache: 1. memory cache, 2. sqlite db cache
    /// </summary>
    public interface ICache
    {
        public void Set(string key, object value, TimeSpan exp);
        public T Get<T>(string key);
        public bool TryGet<T>(string key, out T val);
        public void Remove(string key);

        public Task<byte[]> TryGetDbDataAsync(string key);
        public Task SetDbAsync(string key, object value, TimeSpan exp);
        public Task<T> GetDbAsync<T>(string key) where T : class;
        public Task<T> TryGetDbAsync<T>(string key) where T : class;
        public Task RemoveDbAsync(string key);
    }

    class CacheService : ICache
    {
        private IServiceProvider sp;
        private IMemoryCache memoryCache;

        public CacheService(
            IServiceProvider sp,
            IMemoryCache memoryCache)
        {
            this.sp = sp;
            this.memoryCache = memoryCache;
        }

        public async Task<byte[]> TryGetDbDataAsync(string key)
        {
            using var scope = sp.CreateScope();
            var cacheRepo = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>().GetSimpleRepository<Cache>();

            var c = await cacheRepo
                .Query()
                .Where(t => t.Key == key)
                .SingleOrDefaultAsync();

            if (c == null || c.Expiration < DateTime.UtcNow)
            {
                cacheRepo.ExecuteDelete(t => t.Key == key);
                return null;
            }

            return c?.Data;
        }

        public async Task AddDbAsync(string key, byte[] data, TimeSpan duration)
        {
            DateTime exp = DateTime.UtcNow.Add(duration);

            using (var scope = sp.CreateScope())
            {
                var scopedUow = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>();
                var crepo = scopedUow.GetSimpleRepository<Cache>();

                var existing = crepo.Query().Where(t => t.Key == key).SingleOrDefault();

                if (existing != null)
                {
                    existing.Update(data, exp);
                }
                else
                {
                    var cache = new Cache(key, data, exp);
                    await crepo.CreateAsync(cache);
                }
            }
        }

        public void Set(string key, object value, TimeSpan exp) => memoryCache.Set(key, value, exp);

        public T Get<T>(string key) => memoryCache.Get<T>(key);

        public bool TryGet<T>(string key, out T val) => memoryCache.TryGetValue<T>(key, out val);

        public void Remove(string key) => memoryCache.Remove(key);

        public async Task SetDbAsync(string key, object value, TimeSpan expiration)
        {
            var jsonValue = System.Text.Json.JsonSerializer.Serialize(value);
            var bytes = System.Text.Encoding.UTF8.GetBytes(jsonValue);
            
            await AddDbAsync(key, bytes, expiration);
        }

        public async Task<T> GetDbAsync<T>(string key) where T: class
        {
            var value = await TryGetDbAsync<T>(key);

            if (value == null) throw new InvalidOperationException("key not exists");

            return value;
        }

        public async Task<T> TryGetDbAsync<T>(string key) where T: class
        {
            byte[] data = await TryGetDbDataAsync(key);

            if (data == null) return null;

            var json = System.Text.Encoding.UTF8.GetString(data);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }

        public async Task RemoveDbAsync(string key)
        {
            using var scope = sp.CreateScope();
            var cacheRepo = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>().GetSimpleRepository<Cache>();

            cacheRepo.ExecuteDelete(x => x.Key == key);
        }
    }
}
