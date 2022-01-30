using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class ArgumentAttributeBindState : IArgumentBind
{
    public bool CanDecode(ParameterInfo parameter, object argument)
    {
        return parameter.CustomAttributes.Any(x => x.AttributeType == typeof(StateAttribute));
    }

    public bool CanEncode(ParameterInfo parameter, object argument)
    {
        return parameter.CustomAttributes.Any(x => x.AttributeType == typeof(StateAttribute));
    }

    readonly IKeyValueStorage _store;

    public ArgumentAttributeBindState(IKeyValueStorage store)
    {
        _store = store;
    }

    public async ValueTask<object> Decode(ParameterInfo parameter, object argument, IUpdateContext ctx)
    {
        var userId = ctx.GetSafeUserId()!.Value;
        var attribute = parameter.GetCustomAttribute<StateAttribute>()!;
        var stateKey = attribute.Name ?? parameter.ParameterType.Name;
        var state = await _store.Get(userId, stateKey, attribute.DefauleValue);
        return state!;
    }

    public string Encode(ParameterInfo parameter, object argument, IUpdateContext ctx)
    {
        return ".";
    }
}
