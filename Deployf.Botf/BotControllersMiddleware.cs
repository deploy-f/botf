using Deployf.Botf.Extensions;
using Deployf.Botf.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf.Controllers
{
    public class BotControllersMiddleware : IUpdateHandler
    {
        readonly ILogger<BotControllersMiddleware> _log;
        readonly BotControllerRoutes _map;
        readonly IBotContextAccessor _accessor;

        public BotControllersMiddleware(BotControllerRoutes map, IBotContextAccessor accessor, ILogger<BotControllersMiddleware> log)
        {
            _map = map;
            _accessor = accessor;
            _log = log;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            _accessor.Context = context;
            var payload = context.GetSafeTextPayload();
            if (payload != null)
            {
                string[] entries;
                if (payload[0] == '/')
                {
                    entries = payload.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    entries = payload.Split("/", StringSplitOptions.RemoveEmptyEntries);
                }
                var key = entries[0];

                if (_map.TryGetValue(key, out var value) && value != null)
                {
                    var arguments = entries.Skip(1).ToArray();
                    
                    _log.LogDebug("Found bot action {Controller}.{Method}. Payload: {Payload} Arguments: {@Args}",
                        value.DeclaringType.Name,
                        value.Name,
                        payload,
                        arguments);

                    var controller = (BotControllerBase)context.Services.GetService(value.DeclaringType);
                    controller.Init(context, cancellationToken);
                    context.Items["args"] = arguments;
                    context.Items["action"] = value;
                    context.Items["controller"] = controller;
                }
            }

            await next(context, cancellationToken);
        }
    }
}