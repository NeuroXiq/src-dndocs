using DNDocs.Domain.Entity.App;
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

namespace DNDocs.Infrastructure.Utils
{
    /// <summary>
    /// instance will cache in two ways:
    /// 1. in memory cache (in RAM memory, will increase ram usage)
    /// 2. in database cache (special table in database with 'key' and  'byte[]' data)
    /// invoking specific methods will use specific storage
    /// 'O': will store in 'object in RAM' cache
    /// no 'O': will store in database 'byte[]' data
    /// </summary>
    public interface ICache
    {
        public byte[] GetData(string key);
        public bool TryGetKM(object _this, string paramVariant, out byte[] data, [CallerMemberName] string callerMethodName = null);
        public bool TryGetOKM<TObj>(object _this, string paramVariant, out TObj val, [CallerMemberName] string callerMethodName = null);
        public bool TryGetO<T>(string key, out T val);
        public bool TryGetJKM<TObj>(object _this, string paramVariant, out TObj val, [CallerMemberName] string callerMethodName = null);
        public bool TryGetJ<Obj>(string key, out Obj val);

        /// <summary>
        /// Add with automatic key - use type name and method name as a key with variable 'paramVariant' value (e.g. method parameters)
        /// </summary>
        public void AddOKM<TObj>(object _this, string paramVariant, TObj obj, TimeSpan exp, [CallerMemberName] string callerMethodName = null);
        public void AddKM(object _this, string paramVariant, byte[] data, TimeSpan exp, [CallerMemberName] string callerMethodName = null);
        public void AddJKM(object _this, string paramVariant, object data, TimeSpan exp, [CallerMemberName] string callerMethodName = null);

        public T GetOrAddOKM<T>(object _this, string paramVariant, Func<T> addIfNotExists, TimeSpan exp, [CallerMemberName] string callerMethodName = null);
        public Task<T> GetOrAddOKMAsync<T>(object _this, string paramVariant, Func<Task<T>> addIfNotExists, TimeSpan exp, [CallerMemberName] string callerMethodName = null);

        public void Add(string key, byte[] data, TimeSpan exp);
        public void AddO<T>(string key, T obj, TimeSpan exp);
        public void AddJ<T>(string key, T obj, TimeSpan exp);

        /// <summary>
        /// Utility method formats cache key based on class name, method and parameter name
        /// if caching is performed on specific method
        /// </summary>
        public string Key(string className, string methodName, string paramName);
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

        private string KeyMethod(object _this, string methodName, string variableParam)
        {
            Validation.AppEx(_this == null, $"_this is null, to use cache provide current instance that invoking cache as '_this' paramter or use key as a string explicitly");
            Validation.AppEx(string.IsNullOrWhiteSpace(methodName), "methodname is null or empty");

            return $"{_this.GetType().FullName}_{methodName}_{variableParam}";
        }

        public string Key(string className, string methodName, string paramName)
        {
            Validation.NotStringIsNullOrWhiteSpace(className);
            Validation.NotStringIsNullOrWhiteSpace(methodName);
            Validation.NotStringIsNullOrWhiteSpace(paramName);

            return $"{className}_{methodName}_{paramName}";
        }

        public bool TryGetOKM<TObj>(object _this, string paramVariant, out TObj val, [CallerMemberName] string callerMethodName = null)
        {
            return TryGetO(KeyMethod(_this, callerMethodName, paramVariant), out val);
        }

        public T GetOrAddOKM<T>(object _this, string paramVariant, Func<T> addIfNotExists, TimeSpan exp, [CallerMemberName] string callerMethodName = null)
        {
            T result;

            if (!TryGetOKM<T>(this, paramVariant, out result, callerMethodName))
            {
                T resolvedVal = addIfNotExists();
                AddOKM(this, paramVariant, resolvedVal, exp, callerMethodName);

                return resolvedVal;
            }

            return result;
        }

        public async Task<T> GetOrAddOKMAsync<T>(object _this, string paramVariant, Func<Task<T>> addIfNotExists, TimeSpan exp, [CallerMemberName] string callerMethodName = null)
        {
            T result;

            if (!TryGetOKM<T>(this, paramVariant, out result, callerMethodName))
            {
                T resolvedVal = await addIfNotExists();
                AddOKM(this, paramVariant, resolvedVal, exp, callerMethodName);

                return resolvedVal;
            }

            return result;
        }

        public bool TryGetO<T>(string key, out T value)
        {
            return memoryCache.TryGetValue<T>(key, out value);
        }

        public void AddKM(object _this, string paramVariant, byte[] data, TimeSpan exp, [CallerMemberName] string callerMethodName = null)
        {
            // separate scope, calling method not need to commit
            Add(KeyMethod(_this, callerMethodName, paramVariant), data, exp);
        }

        public byte[] GetData(string key)
        {

            using var scope = sp.CreateScope();
            var cacheRepo = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>().GetSimpleRepository<Cache>();

            var c = cacheRepo
            .Query()
            .Where(t => t.Key == key)
            .SingleOrDefault();

            if (c == null || c.Expiration < DateTime.UtcNow)
            {
                cacheRepo.ExecuteDelete(t => t.Key == key);
                return null;
            }

            return c.Data;
        }

        public void AddOKM<TObj>(object _this, string paramVariant, TObj obj, TimeSpan exp, [CallerMemberName] string callerMethodName = null)
        {
            AddO(KeyMethod(_this, callerMethodName, paramVariant), obj, exp);
        }

        public void Add(string key, byte[] data, TimeSpan duration)
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
                    crepo.Create(cache);
                }
            }
        }

        public bool TryGetKM(object _this, string paramVariant, out byte[] data, [CallerMemberName] string callerMethodName = null)
        {
            data = null;
            var key = KeyMethod(_this, callerMethodName, paramVariant);
            data = GetData(key);

            return data != null;
        }

        public void AddO<TObj>(string key, TObj obj, TimeSpan exp)
        {
            memoryCache.Set<TObj>(key, obj, new DateTimeOffset(DateTime.UtcNow.Add(exp)));
        }

        public bool TryGetJKM<TObj>(object _this, string paramVariant, out TObj val, [CallerMemberName] string callerMethodName = null)
        {
            if (TryGetKM(_this, paramVariant, out var data, callerMethodName))
            {
                val = JsonConvert.DeserializeObject<TObj>(Encoding.UTF8.GetString(data));

                return true;
            }

            val = default(TObj);

            return false;
        }

        public void AddJKM(object _this, string paramVariant, object data, TimeSpan exp, [CallerMemberName] string callerMethodName = null)
        {
            this.AddKM(_this, paramVariant, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)), exp, callerMethodName);
        }


        public void AddJ<T>(string key, T data, TimeSpan exp)
        {
            Add(key, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)), exp);
        }

        public bool TryGetJ<TObj>(string key, out TObj val)
        {
            byte[] data = GetData(key);

            if (data != null)
            {
                val = JsonConvert.DeserializeObject<TObj>(Encoding.UTF8.GetString(data));
                return true;
            }

            val = default(TObj);

            return false;
        }
    }
}
