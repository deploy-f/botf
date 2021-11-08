namespace Deployf.Botf.ScheduleExample;

public class ScheduleController : BotControllerBase
{
    readonly ScheduleService service;

    public ScheduleController(ScheduleService service)
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
    public async Task FillCalendar()
    {
        var from = DateTime.Now.Date.AddDays(1).AddHours(10);
        var to = DateTime.Now.Date.AddDays(1).AddHours(19);
        await service.Add(FromId, new CreateScheduleParams(from, to, 30));

        Push("Filled");
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
            u => ($"{u.From:HH.mm} - {u.To:HH.mm}", Q(Booking, u.Id)),
            Q(ListFreeSchedules, uid64, dt64, "{0}"),
            3
        );
        RowButton("🔙 Back", Q(ListCalendarDays, uid64, 0));
    }

    [Action]
    public void Booking(int scheduleId)
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

        Booking(schedule.Id);
    }

    [Action("/list_all")]
    [Authorize("admin")]
    public async Task ListSchedulers()
    {
        await ListSchedulersPage(0);
    }
}