using Deployf.Botf;
using SQLite;

public class UserService : IBotUserService
{
    readonly TableQuery<User> _users;
    static string[] _zeroRoles = new string[0];
    static UserRole[] _roles = Enum.GetValues<UserRole>();

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
        var roles = GetRoles(user.Roles);
        return new ((id, roles));
    }

    private string[] GetRoles(UserRole roles)
    {
        if(roles == UserRole.none)
        {
            return _zeroRoles;
        }

        return _roles.Where(c => ((int)c & (int)roles) == (int)c)
            .Select(c => c.ToString())
            .ToArray();
    }
}