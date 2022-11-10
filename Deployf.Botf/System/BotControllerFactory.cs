using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public delegate ValueTask<bool> RouteSkipDelegate(string command, RouteInfo<string> info, IUpdateContext context);
public delegate RouteSkipDelegate? RouteSkipFactoryDelegate(bool hasStates, MethodInfo action);

public class BotControllerFactory
{
    private static Type baseController { get; } = typeof(BotController);
    private static List<Type> _controllers { get; } = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(c => c.GetTypes())
            .Where(c => !c.IsAbstract && baseController.IsAssignableFrom(c))
            .ToList();

    public static BotControllerRoutes MakeRoutes(RouteSkipFactoryDelegate skipFactory)
    {
        var stateControllerType = typeof(BotControllerState);
        var hasStateController = _controllers.Any(c => {
            return stateControllerType.IsAssignableFrom(c);
        });

        var keys = _controllers
            .Select(c => c.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(s => (templ: GetTemplates(s), m: s))
                .SelectMany(s => s.templ.Select(f => (templ: f, m: s.m)))
                .Where(s => s.templ != null)
                .Select(s => (templ: s.templ!, m: new RouteInfo<string>(s.m, skipFactory(hasStateController, s.m)))))

            .SelectMany(c => c)
            .ToList();

        return new BotControllerRoutes(keys);
    }

    private static IEnumerable<string> GetTemplates(MethodInfo method)
    {
        var routes = method.GetCustomAttributes<ActionAttribute>();
        return routes.Select(route => route.Template ?? GetAnonymousName(method)); //TODO: Check multiple attributes
        
        static string GetAnonymousName(MethodInfo m)
        {
            var signature = $"{m.DeclaringType!.FullName}_{m}";
            var hash = signature.GetDeterministicHashCode()
                .Base64()
                .Replace('/', '_')
                .TruncateEnd(5);
            return $"${hash}_{m.Name}";
        }
    }


    public static BotControllerStates MakeStates()
    {
        var keys = _controllers
            .Select(c => c.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(c => c.GetParameters().Length == 1)
                .Where(c => c.GetCustomAttribute<StateAttribute>() != null)
                .Select(c => (type: c.GetParameters()[0].ParameterType, m: c)))

            .SelectMany(c => c)

            .ToDictionary(c => c.type, c => c.m);

        return new BotControllerStates(keys);
    }

    public static BotControllerHandlers MakeHandlers()
    {
        var handlers = _controllers
            .Select(c => c.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(c => c.GetCustomAttribute<OnAttribute>() != null)
                .Select(c => (on: c.GetCustomAttribute<OnAttribute>()!, m: c)))

            .SelectMany(c => c)
            .OrderBy(c => c.on.Filter == null)
            .ThenByDescending(c => c.on.Order)

            .Select(c => new HandlerItem(c.on.Handler, c.m, GetFilter(c.m, c.on)));

        return new BotControllerHandlers(handlers);

        static ActionFilter? GetFilter(MethodInfo target, OnAttribute on)
        {
            var filters = target.GetCustomAttributes<FilterAttribute>().ToArray();

            if(filters.Length == 0 && on.Filter == null)
            {
                return null;
            }

            if(filters.Length != 0 && on.Filter != null)
            {
                throw new BotfException("Use only Filter() attribute to pass filter methods of handlers");
            }

            if(on.Filter != null)
            {
                var methodInTarget = FilterAttribute.GetMethod(on.Filter, target.DeclaringType);
                CheckFilterMethod(methodInTarget, on.Filter, target.DeclaringType!.Name);
                return (ActionFilter)ActionFilter.CreateDelegate(typeof(ActionFilter), methodInTarget!);
            }

            if(filters.Length == 0)
            {
                return null;
            }

            ActionFilter? result = null;

            for (int i = 0; i < filters.Length; i++)
            {
                var filter = filters[i];
                var method = filter.GetMethod(target.DeclaringType);
                CheckFilterMethod(method, filter.Filter, target.DeclaringType!.Name);
                var action = (ActionFilter)ActionFilter.CreateDelegate(typeof(ActionFilter), method!);

                if(result == null)
                {
                    if(filter.Operation == FilterAttribute.BoolOp.Not)
                    {
                        result = (IUpdateContext ctx) =>
                        {
                            ctx.SetFilterParameter(filter.Param);
                            return !action(ctx);
                        };
                    }
                    else
                    {
                        result = (IUpdateContext ctx) =>
                        {
                            ctx.SetFilterParameter(filter.Param);
                            return action(ctx);
                        };
                    }
                }
                else
                {
                    var currentResult = result;
                    result = (IUpdateContext ctx) =>
                    {
                        var leftResult = currentResult(ctx);

                        ctx.SetFilterParameter(filter.Param);
                        var rightResult = action(ctx);

                        if(filter.Operation == FilterAttribute.BoolOp.And)
                        {
                            return leftResult && rightResult;
                        }
                        else if(filter.Operation == FilterAttribute.BoolOp.Or)
                        {
                            return leftResult || rightResult;
                        }
                        else if(filter.Operation == FilterAttribute.BoolOp.AndNot)
                        {
                            return leftResult && !rightResult;
                        }
                        else if(filter.Operation == FilterAttribute.BoolOp.OrNot)
                        {
                            return leftResult || !rightResult;
                        }
                        else if(filter.Operation == FilterAttribute.BoolOp.Not)
                        {
                            throw new NotSupportedException($"Operation NOT is supported only for first filter");
                        }

                        throw new NotSupportedException($"Operation type {filter.Operation} is not supported");
                    };
                }
            }

            return result;

            static void CheckFilterMethod(MethodInfo? filter, string methodName, string typeName)
            {
                if(filter == null
                    || filter.ReturnType != typeof(bool)
                    || filter.GetParameters().Length != 1
                    || filter.GetParameters()[0].ParameterType != typeof(IUpdateContext))
                {
                    throw new BotfException($"Filter method name is wrong. Can't find method `{methodName}` in type `{typeName}`. "
                        + "The method must be static and return bool value and receive single argument with type `IUpdateContext`. "
                        + "Action method is `{target.DeclaringType!.Name}.{target.Name}`");
                }
            }
        }
    }
}