namespace Deployf.Botf;

public interface IBotUserService
{
    ValueTask<(string? id, string[]? roles)> GetUserIdWithRoles(long tgUserId);
}

public class BotUserService
{
    readonly IBotUserService? _userService;

    public BotUserService(IBotUserService userService)
    {
        _userService = userService;
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
        return (null, null); ;
    }
}