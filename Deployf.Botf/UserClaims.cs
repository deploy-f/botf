namespace Deployf.Botf;

public class UserClaims
{
    static readonly string[] _emptyRoles = new string[0];

    public bool IsAuthorized { get; set; }
    public string Id { get; set; } = string.Empty;
    public string[] Roles { get; set; } = _emptyRoles;

    public bool IsInRole(string role) => Roles.Contains(role);

    public static implicit operator string(UserClaims user)
    {
        return user.Id;
    }
}