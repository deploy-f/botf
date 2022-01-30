using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;
public class ArgumentBinder
{
    readonly IEnumerable<IArgumentBind> _typeBinds;

    public ArgumentBinder(IEnumerable<IArgumentBind> typeBinds)
    {
        _typeBinds = typeBinds;
    }

    public async ValueTask<object[]> Bind(MethodInfo method, object[] args, IUpdateContext ctx)
    {
        List<object> bindings = new();
        var parameters = method.GetParameters();
        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            var arg = args[i];
            var found = false;
            foreach (var binder in _typeBinds)
            {
                if (binder.CanDecode(p, arg))
                {
                    bindings.Add(await binder.Decode(p, arg, ctx));
                    found = true;
                    continue;
                }
            }

            if(found)
            {
                continue;
            }

            if(p.ParameterType.IsAssignableFrom(arg.GetType()))
            {
                bindings.Add(arg);
                continue;
            }
            
            throw new NotImplementedException($"Binding for parameter {p} for action {method} not found");
        }
        return bindings.ToArray();
    }

    public object[] Convert(MethodInfo method, object[] args, IUpdateContext ctx)
    {
        List<object> bindings = new();
        var parameters = method.GetParameters();
        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            var arg = args[i];
            var found = false;
            foreach (var binder in _typeBinds)
            {
                if (binder.CanEncode(p, arg))
                {
                    bindings.Add(binder.Encode(p, arg, ctx));
                    found = true;
                    continue;
                }
            }

            if (!found)
            {
                throw new NotImplementedException($"Binding for parameter {p} for action {method} not found");
            }
        }

        return bindings.ToArray();
    }
}