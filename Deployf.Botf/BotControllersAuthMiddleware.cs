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
            var user = await GetUser(context);
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

    async Task<UserClaims> GetUser(IUpdateContext context)
    {
        var tgUserId = context.GetSafeUserId();
        if (!tgUserId.HasValue)
        {
            _log.LogWarning("Telegram userId not found!");
            return new UserClaims();
        }

        var (userId, roles) = await _tokenService.GetUserIdWithRoles(tgUserId.Value);
        if (userId == null)
        {
            _log.LogDebug("User with {tgUserId} not found in database", tgUserId.Value);
            return new UserClaims();
        }

        var claim = new UserClaims()
        {
            Roles = roles ?? new string[0],
            IsAuthorized = true,
            Id = userId
        };

        _log.LogDebug("User {@User} with {tgUserId} found", claim, tgUserId.Value);
        return claim;
    }
}