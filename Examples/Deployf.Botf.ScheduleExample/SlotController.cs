namespace Deployf.Botf.ScheduleExample;

public class SlotController : BotControllerBase
{
    readonly SlotService service;

    public SlotController(SlotService service)
    {
        this.service = service;
    }

    [Action("/start")]
    public async Task Deeplink(string uid)
    {
        await ListCalendarDays(uid, 0);
    }

    [Action("/list", "show my slots")]
    public async Task ListMySlots()
    {
        await ListCalendarDays(FromId.Base64(), 0);
    }


    public struct FillState
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

    private FillState GetFillState() => Store!.Get<FillState>("fill", new FillState());
    private void SetFillState(FillState state) => Store!["fill"] = state;
    private string WeekdaysString(WeekDay weekDay)
    {
        var values = Enum.GetValues<WeekDay>();
        var result = string.Empty;
        if(weekDay == 0)
        {
            return "not set";
        }

        for (int i = 0; i < values.Length; i++)
        {
            if((weekDay & values[i]) == values[i])
            {
                result += values[i].ToString()[0..2] + " ";
            }
        }

        return result;
    }

    [Action("/fill", "add a serries slots")]
    public void FillCalendar()
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
    public void Fill_LoopStart(string state)
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
    public void Fill_SetStart(long state)
    {
        var fillState = GetFillState();
        SetFillState(fillState with { Start = DateTime.FromBinary(state) });
        FillCalendar();
    }


    [Action]
    public void Fill_LoopFinish(string state)
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
    public void Fill_SetFinish(string state)
    {
        var fillState = GetFillState();
        SetFillState(fillState with { End = new CalendarState(state).Date() });
        FillCalendar();
    }



    [Action]
    public void Fill_LoopUpTo(string state)
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
    public void Fill_SetUpTo(string state)
    {
        var fillState = GetFillState();
        SetFillState(fillState with { UpTo = new CalendarState(state).Date() });
        FillCalendar();
    }


    [Action]
    public void Fill_LoopWeekDays(int state)
    {
         Push("Peek weekdays");

        var weekdays = (WeekDay)state;

        new FlagMessageBuilder<WeekDay>(weekdays)
            .Navigation(s => Q(Fill_LoopWeekDays, (int)s))
            .Build(Message);

        RowButton("Done", Q(Fill_SetWeekDays, state));
    }

    [Action]
    public void Fill_SetWeekDays(int state)
    {
        var fillState = GetFillState();
        SetFillState(fillState with { WeekDays = (WeekDay)state });
        FillCalendar();
    }

    [Action]
    public void Fill_Comment()
    {
        State(new SetCommentState());
        Push("Send a comment");
    }

    [State]
    public void Fill_StateComment(SetCommentState state)
    {
        var fillState = GetFillState();
        SetFillState(fillState with { Comment = Context.GetSafeTextPayload() });
        FillCalendar();
    }
    public record SetCommentState;



    [Action]
    public async Task Fill()
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
    }

    [Action("/add", "add the time free slot")]
    public void AddSlot()
    {
        AddSlotFrom(".");
    }

    [Action]
    public async Task ListSchedulersPage(int page = 0)
    {
        PushL("Schedulers:");
        var pager = await service.GetSchedulers(new PageFilter { Page = page });
        Pager(pager,
            u => (u.FullName, Q(ListCalendarDays, u.Id.Base64(), 0)),
            Q<int>(ListSchedulersPage, "{0}"),
            1
        );
    }

    [Action]
    public async Task ListCalendarDays(string uid64, int page)
    {
        PushL("Days:");
        var now = DateTime.Now.Date;
        var pager = await service.GetFreeDays(uid64.Base64(), now, new PageFilter { Page = page });
        Pager(pager,
            u => ($"{u:dd.MM.yyyy}", Q(ListFreeSchedules, uid64, u.ToBinary().Base64(), 0)),
            Q(ListCalendarDays, uid64, "{0}")
        );
    }

    [Action]
    public async Task ListFreeSchedules(string uid64, string dt64, int page)
    {
        PushL("Free Slots:");
        var date = DateTime.FromBinary(dt64.Base64());
        var pager = await service.GetFreeSlots(uid64.Base64(), date, new PageFilter { Page = page });
        Pager(pager,
            u => ($"{u.From:HH.mm} - {u.To:HH.mm}", Q(SlotView, u.Id)),
            Q(ListFreeSchedules, uid64, dt64, "{0}"),
            3
        );
        RowButton("🔙 Back", Q(ListCalendarDays, uid64, 0));
    }

    [Action]
    public void SlotView(int scheduleId)
    {
        var schedule = service.Get(scheduleId);

        PushL($"{schedule.State} time slot");

        PushL($"Date: {schedule.From:dd.MM.yyyy}");
        PushL($"Time: {schedule.From:HH:mm} - {schedule.To:HH:mm}");
        if(!string.IsNullOrEmpty(schedule.Comment))
        {
            PushL();
            Push(schedule.Comment);
        }

        if(FromId == schedule.OwnerId)
        {
            if (schedule.State != global::State.Canceled)
            {
                RowButton("Cancel", Q(Cancel, scheduleId));
            }
            if (schedule.State == global::State.Canceled)
            {
                RowButton("Free", Q(Free, scheduleId));
            }
        }
        else
        {
            RowButton("Book", Q(Book, scheduleId));
        }

        RowButton("🔙 Back", Q(ListFreeSchedules, schedule.OwnerId.Base64(), schedule.From.Date.ToBinary().Base64(), 0));
    }

    [Action]
    public async Task Book(int scheduleId)
    {
        await service.Book(scheduleId);
        PushL("Booked");
        SlotView(scheduleId);
    }

    [Action]
    public async Task Cancel(int scheduleId)
    {
        await service.Cancel(scheduleId);
        PushL("Canceled");
        SlotView(scheduleId);
    }

    [Action]
    public async Task Free(int scheduleId)
    {
        await service.Free(scheduleId);
        PushL("Now it is free");
        SlotView(scheduleId);
    }

    [Action]
    public void AddSlotFrom(string state_from)
    {
        var now = DateTime.Now;
        new CalendarMessageBuilder()
            .Year(now.Year).Month(now.Month)
            .Depth(CalendarDepth.Minutes)
            .SetState(state_from)

            .OnNavigatePath(s => Q(AddSlotFrom, s))
            .OnSelectPath(d => Q(AddSlotTo, d.ToBinary().Base64(), "."))

            .SkipTo(now)

            .FormatMinute(d => $"{d:HH:mm}")
            .FormatText((dt, depth, b) =>
            {
                var selection = depth switch
                {
                    CalendarDepth.Years => "year",
                    CalendarDepth.Months => "month",
                    CalendarDepth.Days => "day",
                    CalendarDepth.Hours => "hour",
                    CalendarDepth.Minutes => "minute"
                };
                b.Push($"Select {selection} of the from date");
            })

            .Build(Message, new PagingService());
    }

    [Action]
    public void AddSlotTo(string dt_from, string state_to)
    {
        var from = DateTime.FromBinary(dt_from.Base64());
        new CalendarMessageBuilder()
            .Year(from.Year).Month(from.Month)
            .Depth(CalendarDepth.Minutes)
            .SetState(state_to)

            .OnNavigatePath(s => Q(AddSlotTo, dt_from, s))
            .OnSelectPath(d => Q(LetsAddSlot, dt_from, d.ToBinary().Base64()))

            .SkipTo(from)

            .FormatMinute(d => $"{d:HH:mm}")
            .FormatText((dt, depth, b) =>
            {
                b.PushL($"✅The from date is {from}");

                var selection = depth switch
                {
                    CalendarDepth.Years => "year",
                    CalendarDepth.Months => "month",
                    CalendarDepth.Days => "day",
                    CalendarDepth.Hours => "hour",
                    CalendarDepth.Minutes => "minute"
                };
                b.Push($"Select {selection} of the to date");
            })

            .Build(Message, new PagingService());
    }

    [Action]
    public async Task LetsAddSlot(string dt_from, string dt_to)
    {
        var from = DateTime.FromBinary(dt_from.Base64());
        var to = DateTime.FromBinary(dt_to.Base64());
        var schedule = new Schedule
        {
            OwnerId = FromId,
            State = global::State.Free,
            From = from,
            To = to
        };
        await service.Add(schedule);

        PushL("Slot has been added");

        SlotView(schedule.Id);
    }

    [Action("/list_all")]
    [Authorize("admin")]
    public async Task ListSchedulers()
    {
        await ListSchedulersPage(0);
    }
}