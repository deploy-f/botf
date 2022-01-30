namespace Deployf.Botf;

public interface IKeyValueStorage
{
    ValueTask<T?> Get<T>(long userId, string key, T? defaultValue);
    ValueTask<object?> Get(long userId, string key, object? defaultValue);
    ValueTask Set(long userId, string key, object value);
    ValueTask Remove(long userId, string key);
    ValueTask<bool> Contain(long userId, string key);
}