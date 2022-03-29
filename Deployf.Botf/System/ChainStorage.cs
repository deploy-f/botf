using System.Collections.Concurrent;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class ChainStorage
{
    readonly IDictionary<long, ChainItem?> _chains;

    public ChainStorage()
    {
        _chains = new ConcurrentDictionary<long, ChainItem?>();
    }

    public ChainItem? Get(long id)
    {
        _chains.TryGetValue(id, out var result);
        return result;
    }

    public void Clear(long id)
    {
        if(_chains.ContainsKey(id))
        {
            _chains[id] = null;
            _chains.Remove(id);
        }
    }

    public void Set(long id, ChainItem item)
    {
        _chains[id] = item;
    }

    public record ChainItem(TaskCompletionSource<IUpdateContext> Synchronizator);
}