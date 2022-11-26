using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace Deployf.Botf;

public class GlobalStateService : IGlobalStateService
{
    public readonly IKeyValueStorage Store;
    public readonly IBot Bot;
    public readonly IServiceProvider Provider;
    public readonly BotUserService TokenService;
    public readonly BotControllerHandlers Handlers;
    public readonly BotControllerRoutes Routes;
    public readonly BotControllersInvoker Invoker;

    public GlobalStateService(IKeyValueStorage store, BotfBot bot, IServiceProvider provider, BotUserService tokenService, BotControllerHandlers handlers, BotControllerRoutes routes, BotControllersInvoker invoker)
    {
        Store = store;
        Bot = bot;
        Provider = provider;
        TokenService = tokenService;
        Handlers = handlers;
        Routes = routes;
        Invoker = invoker;
    }

    public async Task SetState<T>(long userId, T newState, bool callEnter = true, bool callLeave = true, CancellationToken cancelToken = default)
    {
        var context = new UpdateContext(Bot, new Update(), Provider)
        {
            UserId = userId,
            ChatId = userId
        };
        var user = await TokenService.GetUser(userId);
        if(newState == null)
        {
            if(await Store!.Contain(userId, Consts.GLOBAL_STATE))
            {
                var oldState = await Store!.Get(userId, Consts.GLOBAL_STATE, null);
                if (callLeave && oldState != null)
                {
                    await Call(true, oldState);
                }
            }
            await Store!.Remove(userId, Consts.GLOBAL_STATE);
            await CallClear();
        }
        else
        {
            if (await Store!.Contain(userId, Consts.GLOBAL_STATE))
            {
                var oldState = await Store!.Get(userId, Consts.GLOBAL_STATE, null);
                if(callEnter && oldState != null)
                {
                    await Call(true, oldState);
                }
            }
            await Store!.Set(userId, Consts.GLOBAL_STATE, newState);
            await Call(false, newState);
        }

        async ValueTask Call(bool leave, object oldState)
        {
            var controllerType = Routes.GetStateType(oldState.GetType());
            if (controllerType != null)
            {
                var controller = (BotControllerState)context!.Services.GetRequiredService(controllerType);
                controller.Init(context, cancelToken);
                controller.User = user;
                await controller.OnBeforeCall();
                if (leave)
                {
                    await controller.OnLeave();
                }
                else
                {
                    await controller.OnEnter();
                }
                await controller.OnAfterCall();
            }
        }

        async ValueTask CallClear()
        {
            var handlersContainer = Handlers;
            var lookup = handlersContainer.GetHandlers(Handle.ClearState);
            if(lookup == null)
            {
                return;
            }
            
            foreach (var handler in lookup)
            {
                if(context.IsHandlingStopRequested())
                {
                    break;
                }
                if(handler.TryFilter(context))
                {
                    await Invoker.Invoke(context, cancelToken, handler.TargetMethod);
                }
            }
        }
    }
}