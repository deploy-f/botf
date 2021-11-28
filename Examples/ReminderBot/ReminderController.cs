namespace Deployf.Botf.ReminderBot;

class ReminderController : BotControllerBase
{
    [Action("/add", "add a reminder")]
    void AddReminder()
    {
        AddReminder(new AddState());
    }

    [Action]
    void AddReminder([State] AddState state)
    {
        View("add.botf.html", state);
    }

    [Action]
    void LoopStart(string state)
    {
        View("loopstart.botf.html", state);
    }

    [Action]
    async ValueTask SetStart(DateTime start, [State] AddState state)
    {
        state.Time = start;
        await AState(state);
        AddReminder(state);
    }

    [Action]
    void LoopTo(string state)
    {
        View("loopstart.botf.html", state);
    }

    [Action]
    void LoopWeekday(string state)
    {
        View("loopstart.botf.html", state);
    }

    [Action]
    void Comment()
    {
    }

    public class AddState
    {
        public DateTime Time;
        public bool IsTimeSet;
        public DateTime To;
        public bool IsToSet;
        public WeekDay WeekDay;
        public string Message;

        public bool IsSet
            => IsTimeSet
            && IsToSet
            && WeekDay != 0
            && string.IsNullOrEmpty(Message);
    }
}

public enum WeekDay
{
    Sunday = 1,
    Monday = 1 << 1,
    Tuesday = 1 << 2,
    Wednesday = 1 << 3,
    Thursday = 1 << 4,
    Friday = 1 << 5,
    Saturday = 1 << 6,
}