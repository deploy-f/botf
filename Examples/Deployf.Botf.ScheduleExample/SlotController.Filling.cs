namespace Deployf.Botf.ScheduleExample;

/// Filling part of slots
partial class SlotController : BotControllerBase
{
    [Action]
    void FillCalendar([State] FillState state)
    {
        PushL("Fill periodically time slot");

        RowButton(state.Start.HasValue ? $"From {state.Start:HH:mm}" : "Set start time", Q(Fill_LoopStart, "."));
        RowButton(state.End.HasValue ? $"To {state.End:HH:mm}" : "Set finish time", Q(Fill_LoopFinish, ".", ""));
        RowButton(state.UpTo.HasValue ? $"Series up to {state.UpTo:MM.dd}" : "Set series end date", Q(Fill_LoopUpTo, "."));
        RowButton(state.WeekDays.HasValue ? $"Repeating {WeekdaysString(state.WeekDays.Value)}" : "Set repeating", Q(Fill_LoopWeekDays, 0));
        RowButton(string.IsNullOrEmpty(state.Comment) ? "Set or update comment" : "Comment: " + state.Comment, Q(Fill_Comment));

        if (state.IsSet)
        {
            RowButton("Schedule", Q(Fill, ""));
        }
    }

    [Action]
    void Fill_LoopStart(string state)
    {
        Push("Peek starts time for a slot");

        var now = DateTime.Now;

        Calendar().Depth(CalendarDepth.Time)
            .SetState(state)

            .OnNavigatePath(s => Q(Fill_LoopStart, s))
            .OnSelectPath((d, s) => Q(Fill_SetStart, d, ""))
            .Build(Message);
    }

    [Action]
    async ValueTask Fill_SetStart(DateTime start, [State] FillState state)
    {;
        state = state with { Start = start };
        await AState(state);
        FillCalendar(state);
    }


    [Action]
    void Fill_LoopFinish(string state, [State] FillState fillState)
    {
        Push("Peek finish time for a slot");

        var now = DateTime.Now;
        Calendar().Depth(CalendarDepth.Time)
            .SkipTo(fillState.Start.GetValueOrDefault(DateTime.Now))
            .SetState(state)

            .OnNavigatePath(s => Q(Fill_LoopFinish, s, ""))
            .OnSelectPath((d, s) => Q(Fill_SetFinish, d, ""))
            .Build(Message);
    }

    [Action]
    async ValueTask Fill_SetFinish(DateTime finish, [State] FillState state)
    {
        state = state with { End = finish };
        await AState(state);
        FillCalendar(state);
    }



    [Action]
    void Fill_LoopUpTo(string state)
    {
        Push("Peek to day");

        var now = DateTime.Now;
        Calendar().Day(null).Depth(CalendarDepth.Date)
            .SkipTo(now)
            .SetState(state)

            .OnNavigatePath(s => Q(Fill_LoopUpTo, s))
            .OnSelectPath((d, s) => Q(Fill_SetUpTo, d, ""))
            .Build(Message);
    }

    [Action]
    async ValueTask Fill_SetUpTo(DateTime upTo, [State] FillState state)
    {
        state = state with { UpTo = upTo };
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
        Push("Send a comment");
    }

    [State]
    async ValueTask Fill_StateComment(SetCommentState state)
    {
        var fillState = await GetAState<FillState>();
        fillState = fillState with { Comment = Context.GetSafeTextPayload() };
        await AState(state);
        FillCalendar(fillState);
    }
    record SetCommentState;


    [Action]
    async ValueTask Fill([State] FillState state)
    {
        await service.AddSeries(new(FromId,
            DateTime.Now.Date,
            state.UpTo!.Value,
            state.WeekDays!.Value,
            state.Start!.Value,
            state.End!.Value));


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
}