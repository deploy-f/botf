using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class BotControllersFSMMiddleware : IUpdateHandler
{
    public const string STATE_KEY = "$store";

    readonly ILogger<BotControllersFSMMiddleware> _log;
    readonly BotControllerStates _map;
    readonly IKeyValueStorage _store;

    public BotControllersFSMMiddleware(ILogger<BotControllersFSMMiddleware> log, BotControllerStates map, IKeyValueStorage store)
    {
        _log = log;
        _map = map;
        _store = store;
    }

    public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
    {
        Func<ValueTask>? afterNext = null;

        var uid = context.GetSafeUserId();
        if (!context.Items.ContainsKey("controller") && uid != null)
        {
            var state = await _store.Get<object>(uid.Value, STATE_KEY, null);

            if (state != null)
            {
                afterNext = () => _store.Remove(uid.Value, STATE_KEY);
            }

            if (state != null && _map.TryGetValue(state.GetType(), out var value) && value != null)
            {
                _log.LogDebug("Found bot state handler {Controller}.{Method}, State: {State}",
                    value.DeclaringType!.Name,
                    value.Name,
                    state);

                var controller = (BotControllerBase)context.Services.GetRequiredService(value.DeclaringType);
                controller.Init(context, cancellationToken);
                context.Items["args"] = new object[] { state };
                context.Items["action"] = value;
                context.Items["controller"] = controller;

                afterNext = async () =>
                {
                    if (state == await _store.Get<object>(uid.Value, STATE_KEY, null))
                    {
                        await _store.Remove(uid.Value, STATE_KEY);
                    }
                };
            }
        }

        await next(context, cancellationToken);
        if(afterNext != null)
        {
            await afterNext();
        }
    }
}