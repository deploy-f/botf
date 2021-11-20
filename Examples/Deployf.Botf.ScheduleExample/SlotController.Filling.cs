namespace Deployf.Botf.ScheduleExample;

/// Filling part of slots
partial class SlotController : BotControllerBase
{
    [Action]
    void FillCalendar()
    {
        var state = GetFillState();

        PushL("Fill periodically time slot");

        RowButton(state.Start.HasValue ? $"From {state.Start:HH:mm}" : "Set start time", Q(Fill_LoopStart, "."));
        RowButton(state.End.HasValue ? $"To {state.End:HH:mm}" : "Set finish time", Q(Fill_LoopFinish, "."));
        RowButton(state.UpTo.HasValue ? $"Series up to {state.UpTo:MM.dd}" : "Set series end date", Q(Fill_LoopUpTo, "."));
        RowButton(state.WeekDays.HasValue ? $"Repeating {WeekdaysString(state.WeekDays.Value)}" : "Set repeating", Q(Fill_LoopWeekDays, 0));
        RowButton(string.IsNullOrEmpty(state.Comment) ? "Set or update comment" : "Comment: " + state.Comment, Q(Fill_Comment));

        if (state.IsSet)
        {
            RowButton("Schedule", Q(Fill));
        }
    }

    [Action]
    void Fill_LoopStart(string state)
    {
        Push("Peek starts time for a slot");

        var now = DateTime.Now;
        new CalendarMessageBuilder()
            .Year(now.Year).Month(now.Month).Day(now.Day)
            .Depth(CalendarDepth.Time)
            .SetState(state)

            .OnNavigatePath(s => Q(Fill_LoopStart, s))
            .OnSelectPath((d, s) => Q(Fill_SetStart, d.ToBinary()))
            .Build(Message);
    }

    [Action]
    void Fill_SetStart(long state)
    {
        var fillState = GetFillState();
        SetFillState(fillState with { Start = DateTime.FromBinary(state) });
        FillCalendar();
    }


    [Action]
    void Fill_LoopFinish(string state)
    {
        Push("Peek finish time for a slot");
        var fillState = GetFillState();

        var now = DateTime.Now;
        new CalendarMessageBuilder()
            .Year(now.Year).Month(now.Month).Day(now.Day)
            .Depth(CalendarDepth.Time)
            .SkipTo(fillState.Start.GetValueOrDefault(DateTime.Now))
            .SetState(state)

            .OnNavigatePath(s => Q(Fill_LoopFinish, s))
            .OnSelectPath((d, s) => Q(Fill_SetFinish, s))
            .Build(Message);
    }

    [Action]
    void Fill_SetFinish(string state)
    {
        var fillState = GetFillState();
        SetFillState(fillState with { End = new CalendarState(state).Date() });
        FillCalendar();
    }



    [Action]
    void Fill_LoopUpTo(string state)
    {
        Push("Peek to day");

        var now = DateTime.Now;
        new CalendarMessageBuilder()
            .Year(now.Year).Month(now.Month)
            .Depth(CalendarDepth.Date)
            .SkipTo(now)
            .SetState(state)

            .OnNavigatePath(s => Q(Fill_LoopUpTo, s))
            .OnSelectPath((d, s) => Q(Fill_SetUpTo, s))
            .Build(Message);
    }

    [Action]
    void Fill_SetUpTo(string state)
    {
        var fillState = GetFillState();
        SetFillState(fillState with { UpTo = new CalendarState(state).Date() });
        FillCalendar();
    }


    [Action]
    void Fill_LoopWeekDays(int state)
    {
        Push("Peek weekdays");

        var weekdays = (WeekDay)state;

        new FlagMessageBuilder<WeekDay>(weekdays)
            .Navigation(s => Q(Fill_LoopWeekDays, (int)s))
            .Build(Message);

        RowButton("Done", Q(Fill_SetWeekDays, state));
    }

    [Action]
    void Fill_SetWeekDays(int state)
    {
        var fillState = GetFillState();
        SetFillState(fillState with { WeekDays = (WeekDay)state });
        FillCalendar();
    }

    [Action]
    void Fill_Comment()
    {
        State(new SetCommentState());
        Push("Send a comment");
    }

    [State]
    void Fill_StateComment(SetCommentState state)
    {
        var fillState = GetFillState();
        SetFillState(fillState with { Comment = Context.GetSafeTextPayload() });
        FillCalendar();
    }
    record SetCommentState;


    [Action]
    async Task Fill()
    {
        var fillState = GetFillState();

        await service.AddSeries(new(FromId,
            DateTime.Now.Date,
            fillState.UpTo!.Value,
            fillState.WeekDays!.Value,
            fillState.Start!.Value,
            fillState.End!.Value));


        PushL("✅ added");
        await ListCalendarDays(FromId.Base64(), 0);

        await AnswerCallback("✅ added");
    }


    struct FillState
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public DateTime? UpTo { get; set; }
        public WeekDay? WeekDays { get; set; }
        public string? Comment { get; set; }

        public bool IsSet => Start.HasValue
            && End.HasValue
            && UpTo.HasValue
            && WeekDays.HasValue
            && !string.IsNullOrEmpty(Comment);
    }

    FillState GetFillState() => Store!.Get<FillState>("fill", new FillState());
    void SetFillState(FillState state) => Store!["fill"] = state;
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
}