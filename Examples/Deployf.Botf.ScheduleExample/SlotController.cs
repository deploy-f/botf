namespace Deployf.Botf.ScheduleExample;

/// <summary>
/// Entypoint to slot commands and management
/// </summary>
public partial class SlotController
{
    readonly SlotService service;

    public SlotController(SlotService service)
    {
        this.service = service;
    }

    [Action("/start")]
    async Task StartDeeplink(string uid)
    {
        await ListCalendarDays(uid, 0);
    }

    [Action("/list", "show my slots")]
    async Task ListMySlots()
    {
        await ListCalendarDays(FromId.Base64(), 0);
    }

    [Action("/fill", "add a serries slots")]
    async ValueTask FillCommand()
    {
        FillCalendar(await GetAState<FillState>());
    }

    [Action("/add", "add the time free slot")]
    void AddSlot()
    {
        AddSlotFrom(".");
    }
}