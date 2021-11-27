using System.Reflection;

namespace Deployf.Botf;

public interface IPathQuery
{
    string GetPath(string controller, string action, params object[] args);
    string GetPath(string path, IDictionary<string, object> args);
}

public class PathQuery : IPathQuery
{
    readonly BotControllerRoutes routes;
    readonly ArgumentBinder binder;
    readonly IBotContextAccessor context;

    public PathQuery(BotControllerRoutes routes, ArgumentBinder binder, IBotContextAccessor context)
    {
        this.routes = routes;
        this.binder = binder;
        this.context = context;
    }

    public string GetPath(string controller, string action, params object[] args)
    {
        var hit = routes.FindTemplate(controller, action, args);
        return MakeRoute(controller, action, args, hit);
    }

    private string MakeRoute(string controller, string action, object[] args, (string? template, MethodInfo? method) hit)
    {
        if (hit.template == null)
        {
            throw new KeyNotFoundException($"Item with controller and action ({controller}, {action}) not found");
        }

        if (hit!.method!.GetParameters().Length != args.Length)
        {
            throw new IndexOutOfRangeException($"Argument lengths not equals");
        }

        var splitter = hit.template.StartsWith("/") ? " " : "/";

        var parts = binder.Convert(hit!.method!, args, context.Context!);
        var part2 = string.Join(splitter, parts);
        return $"{hit.template}{splitter}{part2}".TrimEnd('/').TrimEnd();
    }

    public string GetPath(string path, IDictionary<string, object> args)
    {
        var slice = path.Split("/", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (slice.Length != 2)
        {
            throw new Exception("Path mush be in format 'Controller/Action'");
        }
        
        var controller = slice[0];
        var action = slice[1];

        var hit = routes.FindTemplate(controller, action, args);

        if (hit.template == null)
        {
            throw new KeyNotFoundException($"Item with controller and action ({controller}, {action}) not found");
        }

        var parameters = hit.method!.GetParameters().Select(c => args[c.Name!]).ToArray();

        return MakeRoute(controller, action, parameters, hit);
    }
}