namespace Deployf.Botf
{
    public class UserClaims
    {
        public bool IsAuthorized { get; set; }
        public string Id { get; set; }
        public string[] Roles { get; set; }

        public bool IsInRole(string role) => Roles.Contains(role);

        public static implicit operator string(UserClaims user)
        {
            return user.Id;
        }
    }
}
