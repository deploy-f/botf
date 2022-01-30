using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

//TODO: Refactor to instance class
internal static class RouteStateSkipFunction
{
    static readonly Type STATE_TYPE = typeof(BotControllerState);

    internal static RouteSkipDelegate? SkipFunctionFactory(bool hasStates, MethodInfo action)
    {
        if (!hasStates)
        {
            return null;
        }

        return SkipFunction;
    }

    private static async ValueTask<bool> SkipFunction(string key, RouteInfo<string> info, IUpdateContext ctx)
    {
        var fromId = ctx.GetSafeUserId();
        if (!fromId.HasValue)
        {
            return true;
        }

        var store = ctx.Services.GetRequiredService<IKeyValueStorage>();
        var isStateController = STATE_TYPE.IsAssignableFrom(info.Method.DeclaringType);
        var hasState = await store.Contain(fromId!.Value, Consts.GLOBAL_STATE);
        if (!hasState)
        {
            return isStateController;
        }
        else if(!isStateController)
        {
            return true;
        }

        var state = await store.Get(fromId!.Value, Consts.GLOBAL_STATE, null);
        if (state == null)
        {
            return true;
        }

        var stateType = info.Method.DeclaringType!.BaseType!.GenericTypeArguments[0]; // TODO: implement getting base type
        return !state.GetType().IsAssignableFrom(stateType);
    }
}