using Deployf.Botf;
using SQLite;

public class UserService : IBotUserService
{
    readonly TableQuery<User> _users;
    static string[] _zeroRoles = new string[0];

    public UserService(TableQuery<User> users)
    {
        _users = users;
    }

    public ValueTask<(string? id, string[]? roles)> GetUserIdWithRoles(long tgUserId)
    {
        var user = _users.FirstOrDefault(c => c.Id == tgUserId);
        if (user == null)
        {
            return new ((null, null));
        }


        var id = user.Id.ToString();
        return new ((id, _zeroRoles));
    }
}