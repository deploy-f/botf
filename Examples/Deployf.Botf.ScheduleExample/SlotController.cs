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

    [Action("/fill")]
    public void FillCalendar()
    {
        FillCalendar0(".");
    }

    [Action("/fill")]
    public void FillCalendar0(string s_start)
    {
        Push("Peek starts time for a slot");

        var now = DateTime.Now;
        new CalendarMessageBuilder()
            .Year(now.Year).Month(now.Month).Day(now.Day)
            .Depth(CalendarDepth.Time)
            .SetState(s_start)

            .OnNavigatePath(s => Q(FillCalendar0, s))
            .OnSelectPath((d, s) => Q(FillCalendar1, s, "."))
            .Build(Message);
    }

    [Action("/fill")]
    public void FillCalendar1(string s_start, string s_end)
    {
        Push("Peek end time for a slot");

        var now = DateTime.Now;
        new CalendarMessageBuilder()
            .Year(now.Year).Month(now.Month).Day(now.Day)
            .Depth(CalendarDepth.Time)
            .SkipTo(s_start)
            .SetState(s_end)

            .OnNavigatePath(s => Q(FillCalendar1, s_start, s))
            .OnSelectPath((d, s) => Q(FillCalendar2, s_start, s, "."))
            .Build(Message);
    }

    [Action("/fill")]
    public void FillCalendar2(string s_start, string s_end, string s_upTo)
    {
        Push("Peek to day");

        var now = DateTime.Now;
        new CalendarMessageBuilder()
            .Year(now.Year).Month(now.Month)
            .Depth(CalendarDepth.Date)
            .SkipTo(s_end)
            .SetState(s_upTo)

            .OnNavigatePath(s => Q(FillCalendar2, s_start, s_end, s))
            .OnSelectPath((d, s) => Q(FillCalendar3, s_start, s_end, s, 0))
            .Build(Message);
    }

    [Action("/fill")]
    public void FillCalendar3(string s_start, string s_end, string s_upTo, int weekdays)
    {
        Push("Peek weekdays");

        var state = (WeekDay)weekdays;

        new FlagMessageBuilder<WeekDay>(state)
            .Navigation(s => Q(FillCalendar3, s_start, s_end, s_upTo, (int)s))
            .Build(Message);

        RowButton("Fill", Q(Fill, s_start, s_end, s_upTo, weekdays));
    }

    [Action]
    public async Task Fill(string s_start, string s_end, string s_upTo, int weekdays)
    {
        var start = new CalendarState(s_start).Date();
        var end = new CalendarState(s_end).Date();
        var upTo = new CalendarState(s_upTo).Date();

        await service.AddSeries(new (FromId, DateTime.Now.Date, upTo, (WeekDay)weekdays, start, end));

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

        PushL("You are trying to book slot");

        PushL($"From: {schedule.From}");
        PushL($"To: {schedule.To}");

        Button("Book", Q(Book, scheduleId));
        Button("🔙 Go to list", Q(ListFreeSchedules, schedule.OwnerId.Base64(), schedule.From.Date.ToBinary().Base64(), 0));
    }

    [Action]
    public async Task Book(int scheduleId)
    {
        await service.Book(scheduleId);
        PushL("Booked");
        RowButton("🔙 Go to menu", Q(ListSchedulersPage, 0));

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