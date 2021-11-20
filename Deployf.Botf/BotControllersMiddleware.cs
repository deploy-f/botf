using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class BotControllersMiddleware : IUpdateHandler
{
    readonly ILogger<BotControllersMiddleware> _log;
    readonly BotControllerRoutes _map;
    readonly IBotContextAccessor _accessor;
    readonly IUserKVStorage _storage;

    public BotControllersMiddleware(BotControllerRoutes map, IBotContextAccessor accessor, ILogger<BotControllersMiddleware> log, IUserKVStorage storage)
    {
        _map = map;
        _accessor = accessor;
        _log = log;
        _storage = storage;
    }

    public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
    {
        _accessor.Context = context;

        var uid = context.GetSafeUserId();
        if (uid != null)
        {
            if (!_storage.TryGetValue(uid.Value, out var store))
            {
                store = new KeyValueStorage();
                _storage[uid.Value] = store;
            }

            context.Items["store"] = store;
        }

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
            var arguments = entries.Skip(1).ToArray();

            if (_map.TryGetValue(key, arguments, out var value) && value != null)
            {
                _log.LogDebug("Found bot action {Controller}.{Method}. Payload: {Payload} Arguments: {@Args}",
                    value.DeclaringType!.Name,
                    value.Name,
                    payload,
                    arguments);

                var controller = (BotControllerBase)context.Services.GetRequiredService(value.DeclaringType);
                controller.Init(context, cancellationToken);
                context.Items["args"] = arguments;
                context.Items["action"] = value;
                context.Items["controller"] = controller;
            }
        }

        await next(context, cancellationToken);
    }
}