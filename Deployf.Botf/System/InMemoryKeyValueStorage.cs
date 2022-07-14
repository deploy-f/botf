using System.Collections.Concurrent;
#if !NET5_0
    using ValueTask = System.Threading.Tasks.Task;
#endif

namespace Deployf.Botf;

public class InMemoryKeyValueStorage : IKeyValueStorage
{
    readonly IDictionary<string, object> _store;

    public InMemoryKeyValueStorage()
    {
        _store = new ConcurrentDictionary<string, object>();
    }

#if NET5_0
    public ValueTask<bool> Contain(long userId, string key)
    {
        var realKey = GetRealKey(userId, key);
        return new(_store.ContainsKey(realKey));
    }
#else
    public Task<bool> Contain(long userId, string key)
    {
        var realKey = GetRealKey(userId, key);
        return Task.FromResult(_store.ContainsKey(realKey));
    }
#endif

    
#if NET5_0
        public ValueTask<T?> Get<T>(long userId, string key, T? defaultValue)
    {
        var realKey = GetRealKey(userId, key);
        if(_store.TryGetValue(realKey, out var value))
        {
            return new ((T)value);
        }

        return new (defaultValue);
    }
#else
    public Task<T?> Get<T>(long userId, string key, T? defaultValue)
    {
        var realKey = GetRealKey(userId, key);
        if(_store.TryGetValue(realKey, out var value))
        {
            return Task.FromResult((T)value);
        }

        return Task.FromResult(defaultValue);
    }
#endif

#if NET5_0
        public ValueTask<object?> Get(long userId, string key, object? defaultValue)
    {
        var realKey = GetRealKey(userId, key);
        if (_store.TryGetValue(realKey, out var value))
        {
            return new(value);
        }

        return new(defaultValue);
    }
#else
    public Task<object?> Get(long userId, string key, object? defaultValue)
    {
        var realKey = GetRealKey(userId, key);
        if (_store.TryGetValue(realKey, out var value))
        {
            return Task.FromResult(value);
        }

        return Task.FromResult(defaultValue);
    }
#endif
    public async ValueTask Remove(long userId, string key)
    {
        if(await Contain(userId, key))
        {
            var realKey = GetRealKey(userId, key);
            _store.Remove(realKey);
        }
    }

    public ValueTask Set(long userId, string key, object value)
    {
        var realKey = GetRealKey(userId, key);
        _store[realKey] = value!;
        return ValueTask.CompletedTask;
    }

    string GetRealKey(long userId, string key)
    {
        return userId.ToString() + "/" + key;
    }
}