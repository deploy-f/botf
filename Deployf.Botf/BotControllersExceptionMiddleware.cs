using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class BotControllersExceptionMiddleware : IUpdateHandler
{
    readonly BotControllersInvoker _invoker;
    readonly BotControllerHandlers _handlers;

    public BotControllersExceptionMiddleware(BotControllersInvoker invoker, BotControllerHandlers handlers)
    {
        _invoker = invoker;
        _handlers = handlers;
    }

    public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
    {
        try
        {
            await next(context, cancellationToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            if (_handlers.TryGetValue(Handle.Unauthorized, out var controller))
            {
                await _invoker.Invoke(context, cancellationToken, controller);
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
            if (!_handlers.TryGetValue(Handle.Exception, out var controller))
            {
                throw ex;
            }

            if (controller.GetParameters().Length == 0)
            {
                await _invoker.Invoke(context, cancellationToken, controller);
            }
            else
            {
                await _invoker.Invoke(context, cancellationToken, controller, ex);
            }
        }
    }
}