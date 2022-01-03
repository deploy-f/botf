using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class BotControllersChainMiddleware : IUpdateHandler
{
    readonly ILogger<BotControllersChainMiddleware> _log;
    readonly ChainStorage _chainStorage;

    public BotControllersChainMiddleware(ILogger<BotControllersChainMiddleware> log, ChainStorage chainStorage)
    {
        _log = log;
        _chainStorage = chainStorage;
    }

    public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
    {
        var id = context.GetSafeChatId();
        if (id != null)
        {
            var chain = _chainStorage.Get(id.Value);
            if(chain != null)
            {
                _log.LogTrace("Found chain for user {userId}, triggered continue execution of chain", id.Value);
                _chainStorage.Clear(id.Value);
                if (chain.Synchronizator != null)
                {
                    chain.Synchronizator.SetResult(context);
                }
            }
            else
            {
                await next(context, cancellationToken);
            }
        }
        else
        {
            await next(context, cancellationToken);
        }
    }
}