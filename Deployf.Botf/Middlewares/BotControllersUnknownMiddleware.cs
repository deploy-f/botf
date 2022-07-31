using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class BotControllersUnknownMiddleware : IUpdateHandler
{
    readonly BotControllersInvoker _invoker;
    readonly BotControllerHandlers _handlers;

    public BotControllersUnknownMiddleware(BotControllersInvoker invoker, BotControllerHandlers handlers)
    {
        _invoker = invoker;
        _handlers = handlers;
    }

    public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
    {
        var handlers = _handlers.TryFindHandlers(Handle.Unknown, context);
        var processed = false;
        
        foreach(var handle in handlers)
        {
            if(context.IsHandlingStopRequested())
            {
                break;
            }
            await _invoker.Invoke(context, cancellationToken, handle);
            processed = true;
        }

        if (!processed)
        {
            await next(context, cancellationToken);
        }
    }
}