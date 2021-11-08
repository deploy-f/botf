using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class BotControllersInvoker
{
    readonly ILogger<BotControllersInvoker> _log;
    readonly IServiceProvider _services;

    public BotControllersInvoker(ILogger<BotControllersInvoker> log, IServiceProvider services)
    {
        _log = log;
        _services = services;
    }

    public async Task Invoke(IUpdateContext ctx, CancellationToken token, MethodInfo method, params object[] args)
    {
        var controller = (BotControllerBase?)_services.GetService(method.DeclaringType!);
        controller.Init(ctx, token);
        await InvokeInternal(controller, method, args);
    }

    public async Task<bool> Invoke(IUpdateContext context)
    {
        var isPresented = context.Items.TryGetValue("controller", out var value) && value is BotControllerBase;
        if (!isPresented)
        {
            return false;
        }

        var controller = value as BotControllerBase;

        var method = (MethodInfo)context.Items["action"];
        var args = (object[])context.Items["args"];
        await InvokeInternal(controller, method, args);

        return true;
    }

    private async Task<object?> InvokeInternal(BotControllerBase? controller, MethodInfo method, object[] args)
    {
        var param = method.GetParameters();
        if (args.Length != param.Length)
        {
            throw new IndexOutOfRangeException();
        }

        var typedParams = param.Select((p, i) => (object)(p.ParameterType.Name switch
        {
            nameof(Int32) => int.Parse(args[i].ToString()),
            nameof(Single) => float.Parse(args[i].ToString()),
            _ => MapDefault(p.ParameterType, args[i]),
        })).ToArray();

        _log.LogDebug("Begin execute action {Controller}.{Method}. Arguments: {@Args}",
            method.DeclaringType.Name,
            method.Name,
            typedParams);

        await controller.OnBeforeCall();

        var result = method.Invoke(controller, typedParams);
        if (result is Task task)
        {
            await task;
        }

        await controller.OnAfterCall();

        return result;
    }

    static object MapDefault(Type type, object input)
    {
        if (type.IsAssignableFrom(input.GetType()))
        {
            return input;
        }

        throw new NotImplementedException();
    }
}