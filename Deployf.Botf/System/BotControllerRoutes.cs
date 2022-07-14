using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class RouteInfo<T>
{
    public readonly MethodInfo Method;
    public readonly RouteSkipDelegate? Skip;
    
    public RouteInfo(MethodInfo method, RouteSkipDelegate? skip)
    {
        Method = method;
        Skip = skip;
    }
}

public abstract class BotControllerMap<T> : Dictionary<T, MethodInfo> where T : notnull
{
    public BotControllerMap(IDictionary<T, MethodInfo> data) : base(data)
    {
    }

    public IEnumerable<Type> ControllerTypes()
    {
        return Values
            .Select(c => c.DeclaringType!)
            .Distinct();
    }
}

public abstract class BotControllerListMap<T> : List<(T command, RouteInfo<T> info)> where T : notnull
{
    protected readonly ILookup<T, RouteInfo<T>> _lookup;

    public BotControllerListMap(IList<(T command, RouteInfo<T> info)> data) : base(data)
    {
        _lookup = data.ToLookup(c => c.command, c => c.info);
    }

    public IEnumerable<Type> ControllerTypes()
    {
        return this
            .Select(c => c.info.Method.DeclaringType!)
            .Distinct();
    }
}

public class BotControllerRoutes : BotControllerListMap<string>
{
    static readonly Type STATE_TYPE = typeof(BotControllerState);

    public BotControllerRoutes(IList<(string command, RouteInfo<string> action)> data) :base(data)
    {
    }

    public (string? template, MethodInfo? method) FindTemplate(string controller, string action, object[] args)
    {
        foreach(var item in this)
        {
            if(item.info.Method.Name == action
                && item.info.Method.DeclaringType!.Name == controller
                && args.Length == item.info.Method.GetParameters().Length) //TODO: check the argument types
            {
                return (item.command, item.info.Method);
            }
        }

        return (null, null);
    }

    public async ValueTask<MethodInfo?> GetValue(string key, string[] arguments, IUpdateContext context)
    {
        if(!_lookup.Contains(key))
        {
            return null;
        }

        var targets = _lookup[key];
        foreach (var item in targets)
        {
            if(item.Skip != null && await item.Skip(key, item, context))
            {
                continue;
            }

            if (arguments.Length == item.Method.GetParameters().Length) //TODO: check the argument types
            {
                return item.Method;
            }
        }
        return null;
    }

    public Type? GetStateType(Type stateType)
    {
        return this.Where(c => STATE_TYPE.IsAssignableFrom(c.info.Method.DeclaringType))
            .FirstOrDefault(c => c.info!.Method!.DeclaringType!.BaseType!.GenericTypeArguments[0] == stateType).info?.Method?.DeclaringType;
    }
}

public class BotControllerStates : BotControllerMap<Type>
{
    public BotControllerStates(IDictionary<Type, MethodInfo> data) : base(data)
    {
    }
}

public class BotControllerHandlers : BotControllerMap<Handle>
{
    public BotControllerHandlers(IDictionary<Handle, MethodInfo> data) : base(data)
    {
    }
}