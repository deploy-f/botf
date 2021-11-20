using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class BotControllersFSMMiddleware : IUpdateHandler
{
    readonly ILogger<BotControllersFSMMiddleware> _log;
    readonly BotControllerStates _map;
    readonly IChatFSM _sm;

    public BotControllersFSMMiddleware(ILogger<BotControllersFSMMiddleware> log, BotControllerStates map, IChatFSM sm)
    {
        _log = log;
        _map = map;
        _sm = sm;
    }

    public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
    {
        Action? afterNext = null;

        if (!context.Items.ContainsKey("controller"))
        {
            var state = _sm.Get(context.GetSafeChatId());

            if (state != null)
            {
                afterNext = () => _sm.ClearState(context.GetSafeChatId());
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

                afterNext = () =>
                {
                    if (state == _sm.Get(context.GetSafeChatId()))
                    {
                        _sm.ClearState(context.GetSafeChatId());
                    }
                };
            }
        }

        await next(context, cancellationToken);
        afterNext?.Invoke();
    }
}