using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class ArgumentBindBridge : IArgumentBind
{
    readonly IKeyValueStorage _store;
    readonly IKeyGenerator _keyGenerator;

    public ArgumentBindBridge(IKeyValueStorage store, IKeyGenerator keyGenerator)
    {
        _store = store;
        _keyGenerator = keyGenerator;
    }

    public bool CanDecode(ParameterInfo parameter, object argument)
    {
        return !parameter.ParameterType.IsPrimitive;
    }

    public bool CanEncode(ParameterInfo parameter, object argument)
    {
        return !parameter.ParameterType.IsPrimitive;
    }

    public async ValueTask<object> Decode(ParameterInfo parameter, object argument, IUpdateContext ctx)
    {
        var userId = ctx.GetSafeUserId()!.Value;
        var objectId = "$_bridge_" + argument.ToString()!.Base64();
        var state = await _store.Get(userId, objectId, null);

        // TODO: Check types

        return state!;
    }

    public string Encode(ParameterInfo parameter, object argument, IUpdateContext ctx)
    {
        // TODO: check types

        var userId = ctx.GetSafeUserId()!.Value;
        var id = _keyGenerator.GetLongId();
        var objectId = "$_bridge_" + id;
        _store.Set(userId, objectId, argument);
        return id.Base64();
    }
}