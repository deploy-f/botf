using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf.Controllers
{
    public class BotControllersInvokeMiddleware : IUpdateHandler
    {
        readonly BotControllersInvoker _invoker;

        public BotControllersInvokeMiddleware(BotControllersInvoker invoker)
        {
            _invoker = invoker;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            var invoked = await _invoker.Invoke(context);
            if (!invoked)
            {
                await next(context, cancellationToken);
            }
        }
    }
}