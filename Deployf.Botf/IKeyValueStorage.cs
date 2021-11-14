namespace Deployf.Botf;

public interface IKeyValueStorage : IDictionary<string, object>
{
    T Get<T>(string key, T defaultValue);
}

public class KeyValueStorage : Dictionary<string, object>, IKeyValueStorage
{
    public T Get<T>(string key, T defaultValue)
    {
        if(TryGetValue(key, out var value))
        {
            return (T)value;
        }

        return defaultValue;
    }
}

public interface IUserKVStorage : IDictionary<long, object>
{

}

public class UserKVStorage : Dictionary<long, object>, IUserKVStorage
{
}