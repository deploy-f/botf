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
        var keys = _controllers
            .Select(c => c.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(c => c.GetCustomAttribute<OnAttribute>() != null)
                .Select(c => (type: c.GetCustomAttribute<OnAttribute>()!.Handler, m: c)))

            .SelectMany(c => c)

            .ToDictionary(c => c.type, c => c.m);

        return new BotControllerHandlers(keys);
    }
}