using Deployf.Botf.Extensions;
using Deployf.Botf.Services;
using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf.Controllers
{
    public class BotControllersAuthMiddleware : IUpdateHandler
    {
        readonly ILogger<BotControllersAuthMiddleware> _log;
        readonly IBotUserService _tokenService;

        public BotControllersAuthMiddleware(ILogger<BotControllersAuthMiddleware> log, IBotUserService tokenService)
        {
            _log = log;
            _tokenService = tokenService;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            if(context.Items.TryGetValue("controller", out var value) && value is BotControllerBase controller)
            {
                var method = (MethodInfo)context.Items["action"];
                var user = await GetUser(context);
                context.Items["user"] = user;
                controller.User = user;
                AuthMethod(controller, method);
            }
            await next(context, cancellationToken);
        }

        void AuthMethod(BotControllerBase controller, MethodInfo method)
        {
            var policy = method.GetAuthPolicy();

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
                Roles = roles,
                IsAuthorized = true,
                Id = userId
            };

            _log.LogDebug("User {@User} with {tgUserId} found", claim, tgUserId.Value);
            return claim;
        }
    }
}