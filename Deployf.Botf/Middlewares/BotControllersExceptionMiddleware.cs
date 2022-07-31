using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class BotControllersExceptionMiddleware : IUpdateHandler
{
    readonly BotControllersInvoker _invoker;
    readonly BotControllerHandlers _handlers;
    readonly ILogger<BotControllersExceptionMiddleware> _logger;

    public BotControllersExceptionMiddleware(BotControllersInvoker invoker, BotControllerHandlers handlers, ILogger<BotControllersExceptionMiddleware> logger)
    {
        _invoker = invoker;
        _handlers = handlers;
        _logger = logger;
    }

    public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
    {
        try
        {
            await next(context, cancellationToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            var handlers = _handlers.TryFindHandlers(Handle.Unauthorized, context).ToList();

            if (handlers.Count > 0)
            {
                foreach(var handler in handlers)
                {
                    if(context.IsHandlingStopRequested())
                    {
                        break;
                    }
                    await _invoker.Invoke(context, cancellationToken, handler);
                }
            }
            else
            {
                await ProcessException(ex);
            }
        }
        catch (Exception ex)
        {
            await ProcessException(ex);
        }

        async Task ProcessException(Exception ex)
        {
            var handlers = _handlers.TryFindHandlers(Handle.Exception, context).ToList();

            if (handlers.Count == 0)
            {
                _logger.LogError(ex, "unhandled exception");
                return;
            }

            foreach(var handler in handlers)
            {
                if(context.IsHandlingStopRequested())
                {
                    break;
                }
                if (handler.GetParameters().Length == 0)
                {
                    await _invoker.Invoke(context, cancellationToken, handler);
                }
                else
                {
                    await _invoker.Invoke(context, cancellationToken, handler, ex);
                }
            }
        }
    }
}