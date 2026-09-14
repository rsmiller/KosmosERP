
using KosmosERP.BusinessLayer.Models.Module;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace KosmosERP.BusinessLayer;


public interface IMemoryCacheService<T>
{
    void Setup();
    Task<KeyValueStore?> GetKeyValue(string key_value);
    Task<T?> GetDatabaseValue(int key_value);
    void UpdateDatabaseValue(int id, T obj);
    void UpdateDatabaseValue(string id, T obj);
    Task<T?> GetValue(string key_value);
}

public class MemoryCacheService<T> : IMemoryCacheService<T>
{
    private readonly IMemoryCache _Cache;
    private readonly IBaseERPContext _Context;
    private MemoryCacheEntryOptions _CacheOptions;

    public MemoryCacheService(IMemoryCache cache, IBaseERPContext context)
    {
        _Cache = cache;
        _Context = context;

        _CacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));
    }

    public void Setup()
    {
        var keyValues = _Context.KeyValueStores.Where(m => m.is_deleted == false).ToList();

        foreach (var kvs in keyValues)
        {
            _Cache.Set(kvs.key, kvs, _CacheOptions);
        }
    }

    public async Task<KeyValueStore?> GetKeyValue(string key_value)
    {
        KeyValueStore? kvs;

        if (!_Cache.TryGetValue(key_value, out kvs))
        {
            var dbKsv = await _Context.KeyValueStores.Where(m => m.key == key_value).SingleOrDefaultAsync();

            if (dbKsv != null)
            {
                _Cache.Set(key_value, dbKsv, _CacheOptions);
                kvs = dbKsv;
            }
        }

        return kvs;
    }

    public void UpdateDatabaseValue(int id, T obj)
    {
        if (typeof(T) == typeof(Customer))
        {
            string key_value = "cust-" + id;

            _Cache.Set(key_value, obj, _CacheOptions);
        }

        if (typeof(T) == typeof(OrderHeader))
        {
            string key_value = "oh-" + id;

            _Cache.Set(key_value, obj, _CacheOptions);
        }

        if (typeof(T) == typeof(Product))
        {
            string key_value = "pr-" + id;

            _Cache.Set(key_value, obj, _CacheOptions);
        }
    }


    public void UpdateDatabaseValue(string id, T obj)
    {
        if (typeof(T) == typeof(ModuleObjectDto))
        {
            string key_value = "mod-" + id;

            _Cache.Set(key_value, obj, _CacheOptions);
        }
    }

    public async Task<T?> GetDatabaseValue(int key_value)
    {
        if (typeof(T) == typeof(Customer))
        {
            string key = "cust-" + key_value;

            Customer? cust;

            if (!_Cache.TryGetValue(key, out cust))
            {
                var record = await _Context.Customers.Where(m => m.id == key_value).SingleOrDefaultAsync();

                if (record != null)
                {
                    _Cache.Set(key, record, _CacheOptions);
                    cust = record;

                    return (T)(object)record;
                }
            }

            return (T)(object)cust;
        }

        if (typeof(T) == typeof(OrderHeader))
        {
            string key = "oh-" + key_value;

            OrderHeader? cust;

            if (!_Cache.TryGetValue(key, out cust))
            {
                var record = await _Context.OrderHeaders.Where(m => m.id == key_value).SingleOrDefaultAsync();

                if (record != null)
                {
                    _Cache.Set(key, record, _CacheOptions);
                    cust = record;

                    return (T)(object)record;
                }
            }

            return (T)(object)cust;
        }

        if (typeof(T) == typeof(Product))
        {
            string key = "pr-" + key_value;

            Product? prod;

            if (!_Cache.TryGetValue(key, out prod))
            {
                var record = await _Context.Products.Where(m => m.id == key_value).SingleOrDefaultAsync();

                if (record != null)
                {
                    _Cache.Set(key, record, _CacheOptions);
                    prod = record;

                    return (T)(object)record;
                }
            }

            return (T)(object)prod;
        }

        return default(T);
    }

    public async Task<T?> GetValue(string key_value)
    {
        if (typeof(T) == typeof(ModuleObjectDto))
        {
            string key = "mod-" + key_value;

            ModuleObjectDto? mod;

            _Cache.TryGetValue(key, out mod);

            return (T)(object)mod;
        }

        return default(T);
    }
}