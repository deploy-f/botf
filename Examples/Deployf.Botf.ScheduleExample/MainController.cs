using SQLite;

namespace Deployf.Botf.ScheduleExample;

class MainController : BotControllerBase
{
    readonly TableQuery<User> _users;
    readonly SQLiteConnection _db;
    readonly ILogger<MainController> _logger;

    public MainController(TableQuery<User> users, SQLiteConnection db, ILogger<MainController> logger)
    {
        _users = users;
        _db = db;
        _logger = logger;
    }
    

    [Action("/start")]
    public async Task Start()
    {
        await Send($"Hello!1");
    }

    [Action("/timezone")]
    public async Task Timezone()
    {
        var user = _users.FirstOrDefault(c => c.Id == FromId);

        PushL($"Current time zone: {user.Timezone}");

        Button("Russua", Q(SetTimezone, "ru"));
        Button("Ukraine", Q(SetTimezone, "ua"));
        Button("USA", Q(SetTimezone, "usa"));

        await SetTimezone(null!);

        await Send();
    }

    [Action]
    public async Task SetTimezone(string zone)
    {
        var user = _users.First(c => c.Id == FromId);
        user.Timezone = zone;
        _db.Update(user);

        await AnswerCallback(null);

        await Send("Timezone changed");
    }


    // if user sent unknown action, say it to them
    [On(Handle.Unknown)]
    public async Task Unknown()
    {
        await Send("Unknown command. Or use /start command");
    }

    // handle all messages before botf has processed it
    // and yes, action methods can be void
    [On(Handle.BeforeAll)]
    public void PreHandle()
    {
        // if user has never contacted with the bot we add them to our db at first time
        if(!_users.Any(c => c.Id == FromId))
        {
            var user = new User
            {
                Id = FromId,
                FullName = Context!.GetUserFullName(),
                Username = Context!.GetUsername()!,
                Roles = UserRole.none
            };
            _db.Insert(user);
            _logger.LogInformation("Added user {tgId} at first time", user.Id);
        }
    }

    // handle all errors while message are processing
    [On(Handle.Exception)]
    public async Task OnException(Exception e)
    {
        _logger.LogError(e, "Unhandled exception");
        await Send("Something went wrong");
    }

    // we'll handle auth error if user without roles try use action marked with [Authorize("policy")]
    [On(Handle.Unauthorized)]
    public async Task Forbidden()
    {
        await Send("Forbidden!");
    }
}