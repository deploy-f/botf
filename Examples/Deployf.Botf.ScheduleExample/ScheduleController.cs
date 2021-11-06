namespace Deployf.Botf.ScheduleExample;

public class ScheduleController : BotControllerBase
{
    readonly ScheduleService service;

    public ScheduleController(ScheduleService service)
    {
        this.service = service;
    }

    [Action("/list")]
    public async Task ListSchedulers()
    {
        await ListSchedulersPage(0);
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
        await SendOrUpdate();
    }

    [Action]
    public async Task ListCalendarDays(string uid64, int page)
    {
        PushL("Days:");
        var now = DateTime.Now.Date;
        var pager = await service.GetFreeDays(uid64.Base64(), now, new PageFilter { Page = page });
        Pager(pager,
            u => ($"{u:dd.MM.yyyy}", Q<string, string, int>(ListFreeSchedules, uid64, u.ToBinary().Base64(), 0)),
            Q<string, int>(ListCalendarDays, uid64, "{0}")
        );
        RowButton("Back", Q(ListSchedulersPage, 0));
        await SendOrUpdate();
    }

    [Action]
    public async Task ListFreeSchedules(string uid64, string dt64, int page)
    {
        PushL("Free Slots:");
        var now = DateTime.Now.Date;
        var pager = await service.GetFreeSlots(uid64.Base64(), now, new PageFilter { Page = page });
        Pager(pager,
            u => ($"{u.From:HH.mm} - {u.To:HH.mm}", Q<int>(Booking, u.Id)),
            Q<string, string, int>(ListFreeSchedules, uid64, dt64, "{0}"),
            3
        );
        RowButton("Back", Q(ListCalendarDays, uid64, 0));
        await SendOrUpdate();
    }

    [Action]
    public async Task Booking(int scheduleId)
    {
        var schedule = service.Get(scheduleId);

        PushL("You are trying to book slot");

        PushL($"From: {schedule.From}");
        PushL($"To: {schedule.To}");

        Button("Book", Q(Book, scheduleId));
        Button("Go to list", Q(ListFreeSchedules,schedule.OwnerId.Base64(), schedule.From.Date.ToBinary().Base64(), 0));
        
        await SendOrUpdate();
    }

    [Action]
    public async Task Book(int scheduleId)
    {
        await service.Book(scheduleId);
        PushL("Booked");
        RowButton("Go to menu", Q(ListSchedulersPage, 0));
        await SendOrUpdate();

    }

    [Action("/fill")]
    [Authorize(nameof(UserRole.scheduler))]
    public async Task FillCalendar()
    {
        var from = DateTime.Now.Date.AddDays(1).AddHours(10);
        var to = DateTime.Now.Date.AddDays(1).AddHours(19);
        await service.Add(FromId, new CreateScheduleParams(from, to, 30));

        PushL("Filled");
        await SendOrUpdate();
    }

    public record ScheduleWizardState(long? To, DateTime? StartDate, int? Length, string Comment);
}
