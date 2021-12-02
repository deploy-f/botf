using Hangfire;
using SQLite;

namespace Deployf.Botf.ScheduleExample;

/// <summary>
/// Entypoint to reminder commands and management
/// </summary>
public class ReminderController : BotControllerBase
{
    readonly TableQuery<Reminder> _reminders;
    readonly TableQuery<User> _users;
    readonly SQLiteConnection _connection;

    public ReminderController(TableQuery<Reminder> reminders, SQLiteConnection connection, TableQuery<User> users)
    {
        _reminders = reminders;
        _connection = connection;
        _users = users;
    }


    [Action("/add", "add a serries reminder")]
    public async ValueTask Add()
    {
        FillCalendar(await GetAState<FillState>());
    }

    [Action("/list", "list reminders")]
    public void List()
    {
        ListReminders(0);
    }

    #region Fill
    [Action]
    void FillCalendar([State] FillState state)
    {
        PushL("Fill periodically reminder");

        RowButton(state.Time.HasValue ? $"At {state.Time:HH:mm}" : "Set time", Q(Fill_LoopTime, "."));
        RowButton(state.WeekDays.HasValue ? $"Repeating {WeekdaysString(state.WeekDays.Value)}" : "Set repeating", Q(Fill_LoopWeekDays, 0));
        RowButton(string.IsNullOrEmpty(state.Comment) ? "Set or update comment" : "Comment: " + state.Comment, Q(Fill_Comment));

        if (state.IsSet)
        {
            RowButton("Schedule", Q(Fill, ""));
        }
    }

    [Action]
    void Fill_LoopTime(string state)
    {
        Push("Peek starts time for a slot");

        var now = DateTime.Now;

        Calendar().Depth(CalendarDepth.Time)
            .SetState(state)

            .OnNavigatePath(s => Q(Fill_LoopTime, s))
            .OnSelectPath((d, s) => Q(Fill_SetTime, d, ""))
            .Build(Message);
    }

    [Action]
    async ValueTask Fill_SetTime(DateTime start, [State] FillState state)
    {
        ;
        state = state with { Time = start };
        await AState(state);
        FillCalendar(state);
    }

    [Action]
    void Fill_LoopWeekDays(WeekDay weekdays)
    {
        Push("Peek weekdays");

        new FlagMessageBuilder<WeekDay>(weekdays)
            .Navigation(s => Q(Fill_LoopWeekDays, s))
            .Build(Message);

        RowButton("Done", Q(Fill_SetWeekDays, weekdays, ""));
    }

    [Action]
    async ValueTask Fill_SetWeekDays(WeekDay weekdays, [State] FillState state)
    {
        state = state with { WeekDays = weekdays };
        await AState(state);
        FillCalendar(state);
    }

    [Action]
    void Fill_Comment()
    {
        State(new SetCommentState());
        Push("Reply to this message with a comment");
    }

    [State]
    async ValueTask Fill_StateComment(SetCommentState state)
    {
        var fillState = await GetAState<FillState>();
        fillState = fillState with { Comment = Context.GetSafeTextPayload() };
        await AState(fillState);
        FillCalendar(fillState);
    }
    record SetCommentState;

    [Action]
    async ValueTask Fill([State] FillState state)
    {
        var model = new Reminder
        {
            ChatId = ChatId,
            Time = state.Time!.Value,
            Repeating = state.WeekDays!.Value,
            Comment = state.Comment!
        };
        _connection.Insert(model);

        var user = _users.FirstOrDefault(u => u.Id == FromId);
        var timezoneInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone);
        
        RecurringJob.AddOrUpdate<ReminderJob>($"rem_{model.Id}", j => j.Exec(model), state.GetCron(), timezoneInfo);

        PushL("✅ added");
        ViewReminder(model.Id);
        await AnswerCallback("✅ added");
    }


    struct FillState
    {
        public DateTime? Time { get; set; }
        public WeekDay? WeekDays { get; set; }
        public string? Comment { get; set; }

        public string GetCron()
        {
            var time = $"{Time!.Value.Minute} {Time!.Value.Hour}";
            var date = "* *";
            var dayOfTheWeek = "";

            var enumValues = Enum.GetValues<WeekDay>();
            for (int i = 0; i < enumValues.Length; i++)
            {
                if ((WeekDays!.Value & enumValues[i]) != 0)
                {
                    dayOfTheWeek += i + ",";
                }
            }
            dayOfTheWeek = dayOfTheWeek.Trim(',');

            if (string.IsNullOrEmpty(dayOfTheWeek))
            {
                dayOfTheWeek = "*";
            }

            return $"{time} {date} {dayOfTheWeek}";
        }

        public bool IsSet => Time.HasValue
            && WeekDays.HasValue
            && !string.IsNullOrEmpty(Comment);
    }

    string WeekdaysString(WeekDay weekDay)
    {
        var values = Enum.GetValues<WeekDay>();
        var result = string.Empty;
        if (weekDay == 0)
        {
            return "not set";
        }

        for (int i = 0; i < values.Length; i++)
        {
            if ((weekDay & values[i]) == values[i])
            {
                result += values[i].ToString()[0..2] + " ";
            }
        }

        return result;
    }

    CalendarMessageBuilder Calendar()
    {
        var now = DateTime.Now;
        return new CalendarMessageBuilder()
            .Year(now.Year)
            .Month(now.Month)
            .Day(now.Day);
        //.Culture(CultureInfo.GetCultureInfo("uk-UA"));
    }
    #endregion

    #region List

    [Action]
    void ListReminders(int pageNumber)
    {
        var query = _reminders.AsQueryable().Where(c => c.ChatId == ChatId);
        var pager = new PagingService();
        var page = pager.Paging(query, new PageFilter() { Page = pageNumber });

        Push("Reminders for this chat");

        Pager(page, i => (i.Comment ?? i.Id.ToString(), Q(ViewReminder, i.Id)), Q(ListReminders, "{0}"));

        RowButton("Back to menu", Q<MainController>(c => c.Start));
    }

    [Action]
    void ViewReminder(int reminderId)
    {
        var reminder = _reminders.FirstOrDefault(c => c.Id == reminderId && c.ChatId == ChatId);
        if(reminder == null)
        {
            Push("Not found");
            Button("Back to menu", Q<MainController>(c => c.Start));
            return;
        }

        PushL($"Time: {reminder.Time:HH:mm}");
        PushL($"Comment: {reminder.Comment}");
        Button("Delete", Q(Delete, reminder.Id));
        Button("Back", Q(List));
    }

    [Action]
    async ValueTask Delete(int reminderId)
    {
        var reminder = _reminders.FirstOrDefault(c => c.Id == reminderId && c.ChatId == ChatId);
        if (reminder == null)
        {
            Push("Not found");
            Button("Back to menu", Q<MainController>(c => c.Start));
            return;
        }

        RecurringJob.RemoveIfExists($"rem_{reminder.Id}");

        _connection.Delete(reminder);

        PushL("Deleted");
        List();
        await AnswerCallback("Deleted");
    }

    #endregion
}