using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class BotControllersInvoker
{
    readonly ILogger<BotControllersInvoker> _log;
    readonly IServiceProvider _services;
    readonly ArgumentBinder _binder;

    public BotControllersInvoker(ILogger<BotControllersInvoker> log, IServiceProvider services, ArgumentBinder binder)
    {
        _log = log;
        _services = services;
        _binder = binder;
    }

    public async ValueTask Invoke(IUpdateContext ctx, CancellationToken token, MethodInfo method, params object[] args)
    {
        var controller = (BotController)_services.GetRequiredService(method.DeclaringType!);
        controller.Init(ctx, token);
        await InvokeInternal(controller, method, args, ctx);
    }

    public async ValueTask<bool> Invoke(IUpdateContext context)
    {
        var isPresented = context.Items.TryGetValue("controller", out var value) && value is BotController;
        if (!isPresented)
        {
            return false;
        }

        var controller = (BotController)value!;

        var method = (MethodInfo)context.Items["action"];
        var args = (object[])context.Items["args"];
        await InvokeInternal(controller, method, args, context);

        return true;
    }

    private async ValueTask<object?> InvokeInternal(BotController controller, MethodInfo method, object[] args, IUpdateContext ctx)
    {
        var typedParams = await _binder.Bind(method, args, ctx);

        _log.LogDebug("Begin execute action {Controller}.{Method}. Arguments: {@Args}",
            method.DeclaringType!.Name,
            method.Name,
            typedParams);

        await controller.OnBeforeCall();

        var result = method.Invoke(controller, typedParams);
        if (result is Task task)
        {
            await task;
        }
        else if (result is ValueTask valueTask)
        {
            await valueTask;
        }

        await controller.OnAfterCall();

        return result;
    }
}