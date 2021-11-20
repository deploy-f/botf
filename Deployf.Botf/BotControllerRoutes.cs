using System.Reflection;

namespace Deployf.Botf;

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

public abstract class BotControllerListMap<T> : List<(T command, MethodInfo action)> where T : notnull
{
    public BotControllerListMap(IList<(T, MethodInfo)> data) : base(data)
    {
    }

    public IEnumerable<Type> ControllerTypes()
    {
        return this
            .Select(c => c.action.DeclaringType!)
            .Distinct();
    }
}

public class BotControllerRoutes : BotControllerListMap<string>
{
    public BotControllerRoutes(IList<(string command, MethodInfo action)> data) :base(data)
    {
    }

    public (string? template, MethodInfo? method) FindTemplate(string controller, string action, object[] args)
    {
        foreach(var item in this)
        {
            if(item.action.Name == action
                && item.action.DeclaringType!.Name == controller
                && args.Length == item.action.GetParameters().Length) //TODO: check the argument types
            {
                return (item.command, item.action);
            }
        }

        return (null, null);
    }

    public bool TryGetValue(string key, string[] arguments, out MethodInfo method)
    {
        foreach (var item in this)
        {
            if (item.command == key
                && arguments.Length == item.action.GetParameters().Length) //TODO: check the argument types
            {
                method = item.action;
                return true;
            }
        }

        method = null!;
        return false;
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