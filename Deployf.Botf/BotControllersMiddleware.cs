using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;

namespace Deployf.Botf;

public class BotControllersMiddleware : IUpdateHandler
{
    readonly ILogger<BotControllersMiddleware> _log;
    readonly BotControllerRoutes _map;
    readonly IBotContextAccessor _accessor;
    readonly BotfOptions _opts;

    public BotControllersMiddleware(BotControllerRoutes map, IBotContextAccessor accessor, ILogger<BotControllersMiddleware> log, BotfOptions opts)
    {
        _map = map;
        _accessor = accessor;
        _log = log;
        _opts = opts;
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

            var key = ExtractGroupKey(context, entries[0]);

            var arguments = entries.Skip(1).ToArray();
            var value = await _map.GetValue(key, arguments, context);
            if (value != null)
            {
                _log.LogDebug("Found bot action {Controller}.{Method}. Payload: {Payload} Arguments: {@Args}",
                    value.DeclaringType!.Name,
                    value.Name,
                    payload,
                    arguments);

                var controller = (BotController)context.Services.GetRequiredService(value.DeclaringType);
                controller.Init(context, cancellationToken);
                context.Items["args"] = arguments;
                context.Items["action"] = value;
                context.Items["controller"] = controller;
            }
        }

        await next(context, cancellationToken);
    }

    private string ExtractGroupKey(IUpdateContext context, string key)
    {
        var updateType = context.Update.Type;
        var message = context.Update.Message ?? context.Update.EditedMessage;
        // detect commands in chats like "/command@botname ..."
        if (key[0] == '/'
            && (updateType == UpdateType.EditedMessage || updateType == UpdateType.Message)
            && message!.Chat.Id != message.From!.Id
            && key.Contains('@')
            && key.Count(CharEqualDog) == 1
            && key.EndsWith(_opts.UsernameTag!))
        {
            var tuple = key.Split('@', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            key = tuple[0];
        }

        return key;
    }

    static bool CharEqualDog(char c) => c == '@';
}