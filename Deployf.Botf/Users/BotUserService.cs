using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class BotUserService
{
    readonly IBotUserService? _userService;
    private readonly ILogger<BotUserService> _log;
    
    public BotUserService(IBotUserService? userService, ILogger<BotUserService> log)
    {
        _userService = userService;
        _log = log;
    }

    public BotUserService(ILogger<BotUserService> log)
    {
        _log = log;
    }
    
    public BotUserService()
    {
    }

    public async ValueTask<(string? id, string[]? roles)> GetUserIdWithRoles(long tgUserId)
    {
        if(_userService != null)
        {
            return await _userService.GetUserIdWithRoles(tgUserId);
        }
        return (null, null);
    }
    
    public async Task<UserClaims> GetUser(long? tgUserId)
    {
        if (!tgUserId.HasValue)
        {
            _log.LogWarning("Telegram userId not found!");
            return new UserClaims();
        }

        var (userId, roles) = await GetUserIdWithRoles(tgUserId.Value);
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