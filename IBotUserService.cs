namespace Deployf.TgBot.Services
{
    public interface IBotUserService
    {
        ValueTask<(string id, string[] roles)> GetUserIdWithRoles(long tgUserId);
    }

    public class BotUserService : IBotUserService
    {
        public async ValueTask<(string id, string[] roles)> GetUserIdWithRoles(long tgUserId)
        {
            return (null, null); ;
        }
    }
}