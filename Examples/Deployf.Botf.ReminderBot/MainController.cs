using SQLite;
using Telegram.Bot.Types.Enums;

namespace Deployf.Botf.ScheduleExample;

class MainController : BotController
{
    readonly TableQuery<User> _users;
    readonly SQLiteConnection _db;
    readonly ILogger<MainController> _logger;
    readonly BotfOptions _options;

    static IReadOnlyCollection<TimeZoneInfo> _timeZones = TimeZoneInfo.GetSystemTimeZones();

    public MainController(TableQuery<User> users, SQLiteConnection db, ILogger<MainController> logger, BotfOptions options)
    {
        _users = users;
        _db = db;
        _logger = logger;
        _options = options;
    }


    [Action("/start", "start the bot")]
    public void Start()
    {
        PushL("Hello!");
        PushL("This bot allow you adding a recurring reminder to chat");
        PushL();
        RowButton("Add a reminder", Q<ReminderController>(c => c.Add));
        RowButton("List reminders", Q<ReminderController>(c => c.List));
        RowButton("Set your timezone", Q(ListTimezonesCmd));
    }

    [Action("/timezone")]
    public void ListTimezonesCmd()
    {
        ListTimezones(0);
    }

    [Action]
    public void ListTimezones(int page)
    {
        var user =_users.First(c => c.Id == FromId);
        Push($"Current timezone: {user.TimeZone}");

        var pager = new PagingService();
        var query = _timeZones.Select((c, i) => new { zone = c, index = i }).AsQueryable();
        var pageModel = pager.Paging(query, new PageFilter { Count = 10, Page = page });
        Pager(pageModel, i => (i.zone.DisplayName, Q(SetTimezone, i.index)), Q(ListTimezones, "{0}"), 1);
    }

    [Action]
    public void SetTimezone(int zone)
    {
        var user = _users.First(c => c.Id == FromId);
        user.TimeZone = _timeZones.ElementAt(zone).Id;
        _db.Update(user);

        Push("Timezone has been setted");
        Button("Back to main menu", Q(Start));
    }


    // if user sent unknown action, say it to them
    [On(Handle.Unknown)]
    public void Unknown()
    {
        Push("Unknown command. Or use /start command");
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
                TimeZone = TimeZoneInfo.Local.Id
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
        if (Context.Update.Type == UpdateType.CallbackQuery)
        {
            await AnswerCallback("Error");
        }
        else if (Context.Update.Type == UpdateType.Message)
        {
            Push("Error");
        }
    }

    // we'll handle auth error if user without roles try use action marked with [Authorize("policy")]
    [On(Handle.Unauthorized)]
    public void Forbidden()
    {
        Push("Forbidden!");
    }
}