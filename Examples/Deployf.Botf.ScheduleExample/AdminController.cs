using Deployf.Botf;
using SQLite;

class AdminController : BotControllerBase
{
    readonly TableQuery<User> _users;
    readonly SQLiteConnection _db;

    public AdminController(TableQuery<User> users, SQLiteConnection db)
    {
        _users = users;
        _db = db;
    }

    // it needs to avoid any configuration for specifying admins
    // it's difficult process, you need to find your telegram's user id
    // but this way allow us to became admin without any configuration.
    // you need just call this command in telegram at first, and we're admin now
    [Action("/i_am_admin")]
    public async Task IAmAdmin()
    {
        if(_users.Any(c => (c.Roles & UserRole.admin) == UserRole.admin))
        {
            await Send("No-no-no. You can't do it!");
            return;
        }

        var user = _users.FirstOrDefault(c => c.Id == FromId);
        user.Roles = user.Roles | UserRole.admin; // just set new role with bitflag feature in c#
        _db.Update(user);

        await Send($"You are admin now!");
    }

    // this command only for check, am i admin now or not.
    // Authorize attribute will do all work for ass
    [Action("/am_i_admin")]
    [Authorize("admin")]
    public async Task AmIAdmin()
    {
        await Send($"You are admin");
    }

    [Action("/set_scheduler")]
    [Authorize("admin")]
    public async Task SetScheduler()
    {
        State(new SetSchedulerState());
        await Send($"Text me telegram user's username:");
    }

    public record SetSchedulerState();
    [State]
    public async Task HandleSchedulerState(SetSchedulerState _)
    {
        var payload = Context!.GetSafeTextPayload();
        var user = _users.FirstOrDefault(c => c.Username == payload);
        if(user == null)
        {
            await Send("User not found");
            return;
        }
        user.Roles = user.Roles | UserRole.scheduler;
        _db.Update(user);
        await Send("User became an scheduler");
    }
}