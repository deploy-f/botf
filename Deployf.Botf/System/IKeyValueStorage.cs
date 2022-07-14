namespace Deployf.Botf;
#if NET5_0
    using ValueTaskGeneric = System.Threading.Tasks.ValueTask<object>;
#else
    using ValueTask = System.Threading.Tasks.Task;
    using ValueTaskGeneric = System.Threading.Tasks.Task<object>;
#endif

public interface IKeyValueStorage
{
#if NET5_0
    ValueTask<T?> Get<T>(long userId, string key, T? defaultValue);
#else
    Task<T?> Get<T>(long userId, string key, T? defaultValue);
#endif
    
#if NET5_0
    ValueTask<object?> Get(long userId, string key, object? defaultValue);
#else
    Task<object?> Get(long userId, string key, object? defaultValue);
#endif
    
    ValueTask Set(long userId, string key, object value);
    ValueTask Remove(long userId, string key);
    
#if NET5_0
    ValueTask<bool> Contain(long userId, string key);
#else
    Task<bool> Contain(long userId, string key);
#endif
}