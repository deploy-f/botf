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

            .Select(c => new HandlerItem(c.on.Handler, c.m, GetFilter(c.m, c.on.Filter)));

        return new BotControllerHandlers(handlers);

        static ActionFilter? GetFilter(MethodInfo target, string? filterMethod)
        {
            if(filterMethod == null)
            {
                return null;
            }

            if(filterMethod.Contains('.'))
            {
                var typeName = filterMethod.Substring(0, filterMethod.LastIndexOf('.'));
                var methodName = filterMethod.Substring(filterMethod.LastIndexOf('.') + 1);

                var type = Type.GetType(typeName);
                if(type == null)
                {
                    throw new BotfException($"Filter method name is wrong. Can't find class type `{typeName}`. Action method is `{target.DeclaringType!.Name}.{target.Name}`");
                }

                var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                CheckFilterMethod(method, methodName, typeName, target);

                return (ActionFilter)ActionFilter.CreateDelegate(typeof(ActionFilter), method!);
            }
            
            var methodInTarget = target.DeclaringType!.GetMethod(filterMethod, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            CheckFilterMethod(methodInTarget, filterMethod, target.DeclaringType.Name, target);

            return (ActionFilter)ActionFilter.CreateDelegate(typeof(ActionFilter), methodInTarget!);

            static void CheckFilterMethod(MethodInfo? filter, string methodName, string typeName, MethodInfo target)
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