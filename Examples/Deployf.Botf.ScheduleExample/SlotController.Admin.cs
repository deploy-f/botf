namespace Deployf.Botf.ScheduleExample;

/// Admin part of slot management
partial class SlotController
{
    [Action("/list_all"), Authorize("admin")]
    async Task ListSchedulers()
    {
        await ListSchedulersPage(0);
    }

    [Action, Authorize("admin")]
    async Task ListSchedulersPage(int page = 0)
    {
        PushL("Schedulers:");
        var pager = await service.GetSchedulers(new PageFilter { Page = page });
        Pager(pager,
            u => (u.FullName, Q(ListCalendarDays, u.Id.Base64(), 0)),
            Q<int>(ListSchedulersPage, "{0}"),
            1
        );
    }
}