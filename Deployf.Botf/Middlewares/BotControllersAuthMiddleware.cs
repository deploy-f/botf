using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class BotControllersAuthMiddleware : IUpdateHandler
{
    readonly ILogger<BotControllersAuthMiddleware> _log;
    readonly BotUserService _tokenService;

    public BotControllersAuthMiddleware(ILogger<BotControllersAuthMiddleware> log, BotUserService tokenService)
    {
        _log = log;
        _tokenService = tokenService;
    }

    public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
    {
        if(context.Items.TryGetValue("controller", out var value) && value is BotController controller)
        {
            var method = (MethodInfo)context.Items["action"];
            var user = await _tokenService.GetUser(context.GetSafeUserId());
            context.Items["user"] = user;
            controller.User = user;
            AuthMethod(controller, method);
        }
        await next(context, cancellationToken);
    }

    void AuthMethod(BotController controller, MethodInfo method)
    {
        var policy = method.GetAuthPolicy()?.Trim();

        if(policy == null)
        {
            _log.LogDebug("Authorization skipping");
            return;
        }

        if(policy == string.Empty && !controller.User.IsAuthorized)
        {
            throw new UnauthorizedAccessException();
        }

        if (policy == "admin" && !controller.User.IsInRole("admin"))
        {
            throw new UnauthorizedAccessException("you are not admin");
        }

        if(!string.IsNullOrEmpty(policy) && !controller.User.IsInRole(policy))
        {
            throw new UnauthorizedAccessException();
        }
    }
}