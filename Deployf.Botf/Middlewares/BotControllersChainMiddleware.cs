using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class BotControllersChainMiddleware : IUpdateHandler
{
    readonly ILogger<BotControllersChainMiddleware> _log;
    readonly ChainStorage _chainStorage;
    readonly BotControllersInvoker _invoker;
    readonly BotControllerHandlers _handlers;

    public BotControllersChainMiddleware(ILogger<BotControllersChainMiddleware> log, ChainStorage chainStorage, BotControllersInvoker invoker, BotControllerHandlers handlers)
    {
        _log = log;
        _chainStorage = chainStorage;
        _invoker = invoker;
        _handlers = handlers;
    }

    public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
    {
        try
        {
            var id = context.GetSafeChatId();
            if (id != null)
            {
                var chain = _chainStorage.Get(id.Value);
                if (chain != null)
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
        catch(ChainTimeoutException e)
        {
            _log.LogDebug("Chain timeout reached for chat {chatId}", context.GetChatId());
            if (!e.Handled)
            {
                var handlers = _handlers.TryFindHandlers(Handle.ChainTimeout, context);
                foreach(var handler in handlers)
                {
                    if(context.IsHandlingStopRequested())
                    {
                        break;
                    }
                    await _invoker.Invoke(context, cancellationToken, handler);
                }
            }
        }
    }
}