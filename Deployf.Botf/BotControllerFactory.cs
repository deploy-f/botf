using System.Reflection;

namespace Deployf.Botf;

public class BotControllerFactory
{
    private static Type baseController { get; } = typeof(BotControllerBase);
    private static List<Type> _controllers { get; } = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(c => c.GetTypes())
            .Where(c => !c.IsAbstract && baseController.IsAssignableFrom(c))
            .ToList();

    public static BotControllerRoutes MakeRoutes()
    {
        var keys = _controllers
            .Select(c => c.GetMethods()
                .Select(c => (templ: GetTemplate(c), m: c))
                .Where(c => c.templ != null))

            .SelectMany(c => c)
            .ToList();

        return new BotControllerRoutes(keys);
    }

    private static string? GetTemplate(MethodInfo method)
    {
        var route = method.GetCustomAttribute<ActionAttribute>();
        if(route != null)
        {
            return route.Template ?? method.Name;
        }

        return null;
    }

    public static BotControllerStates MakeStates()
    {
        var keys = _controllers
            .Select(c => c.GetMethods()
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
            .Select(c => c.GetMethods()
                .Where(c => c.GetCustomAttribute<OnAttribute>() != null)
                .Select(c => (type: c.GetCustomAttribute<OnAttribute>().Handler, m: c)))

            .SelectMany(c => c)

            .ToDictionary(c => c.type, c => c.m);

        return new BotControllerHandlers(keys);
    }
}