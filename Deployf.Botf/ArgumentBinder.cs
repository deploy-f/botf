using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public interface IArgumentBind
{
    bool CanDecode(ParameterInfo parameter, object argument);
    bool CanEncode(ParameterInfo parameter, object argument);

    string Encode(ParameterInfo parameter, object argument, IUpdateContext context);
    ValueTask<object> Decode(ParameterInfo parameter, object argument, IUpdateContext context);
}

public class ArgumentBindInt32 : IArgumentBind
{
    public bool CanDecode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(int);
    }

    public bool CanEncode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(int);
    }

    public ValueTask<object> Decode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        return new (int.Parse(argument.ToString()!));
    }

    public string Encode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        return argument.ToString()!;
    }
}

public class ArgumentBindInt64 : IArgumentBind
{
    public bool CanDecode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(long);
    }

    public bool CanEncode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(long);
    }

    public ValueTask<object> Decode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        return new (long.Parse(argument.ToString()!));
    }

    public string Encode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        return argument.ToString()!;
    }
}

public class ArgumentBindSingle : IArgumentBind
{
    public bool CanDecode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(float);
    }

    public bool CanEncode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(float);
    }

    public ValueTask<object> Decode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        return new (float.Parse(argument.ToString()!));
    }

    public string Encode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        return argument.ToString()!;
    }
}

public class ArgumentBindString : IArgumentBind
{
    public bool CanDecode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(string);
    }

    public bool CanEncode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(string);
    }

    public ValueTask<object> Decode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        return new (argument);
    }

    public string Encode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        return argument.ToString()!;
    }
}

public class ArgumentBindEnum : IArgumentBind
{
    public bool CanDecode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType.IsEnum;
    }

    public bool CanEncode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType.IsEnum;
    }

    public ValueTask<object> Decode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        var str = argument.ToString();
        if (Enum.TryParse(parameter.ParameterType, str, out var result))
        {
            return new(result!);
        }
        return ValueTask.FromException<object>(new NotImplementedException("enum conversion for current data is not implemented"));
    }

    public string Encode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        return Enum.Format(parameter.ParameterType, argument, "0");
    }
}

public class ArgumentBindDateTime : IArgumentBind
{
    public bool CanDecode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(DateTime);
    }

    public bool CanEncode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(DateTime);
    }

    public ValueTask<object> Decode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        if (argument is string str)
        {
            var binary = long.Parse(str);
            return new(DateTime.FromBinary(binary));
        }
        return ValueTask.FromException<object>(new NotImplementedException("not implemented convertion"));
    }

    public string Encode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        if (argument is DateTime dt)
        {
            var binary = dt.ToBinary();
            return binary.ToString();
        }
        throw new NotImplementedException("not implemented convertion");
    }
}

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