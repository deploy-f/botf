using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class BotControllersBeforeAllMiddleware : IUpdateHandler
{
    readonly BotControllersInvoker _invoker;
    readonly BotControllerHandlers _handlers;

    public BotControllersBeforeAllMiddleware(BotControllersInvoker invoker, BotControllerHandlers handlers)
    {
        _invoker = invoker;
        _handlers = handlers;
    }

    public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
    {
        if (_handlers.TryGetValue(Handle.BeforeAll, out var controller))
        {
            await _invoker.Invoke(context, cancellationToken, controller);
        }

        await next(context, cancellationToken);
    }
}