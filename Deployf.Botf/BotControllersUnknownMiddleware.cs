using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf.Controllers
{
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
            if (_handlers.TryGetValue(Handle.Unknown, out var controller))
            {
                await _invoker.Invoke(context, cancellationToken, controller);
            }
            else
            {
                await next(context, cancellationToken);
            }
        }
    }
}