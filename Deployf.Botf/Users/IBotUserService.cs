namespace Deployf.Botf;

public interface IBotUserService
{
    ValueTask<(string? id, string[]? roles)> GetUserIdWithRoles(long tgUserId);
}