using Deployf.Botf;
using System.Diagnostics;

class Program : BotfProgram
{
    public static void Main(string[] args) => StartBot(args);

    readonly PagingService pagingService;
    public Program(PagingService pagingService)
    {
        this.pagingService = pagingService;
    }

    [Action("/start")]
    public async Task Start()
    {
        await YearSelect("");
    }

    [Action]
    public async Task YearSelect(string state)
    {
        PushL("Pick the time");

        var now = DateTime.Now;
        new CalendarMessageBuilder()
            .Year(now.Year).Month(now.Month).Day(now.Day)
            .Depth(CalendarDepth.Days)
            .SetState(state)

            .OnNavigatePath(s => Q(YearSelect, s))
            .OnSelectPath(d => Q(DT, d.ToBinary().Base64()))

            .SkipHour(d => d.Hour < 10 || d.Hour > 19)
            .SkipDay(d => d.DayOfWeek == DayOfWeek.Sunday || d.DayOfWeek == DayOfWeek.Saturday)
            .SkipMinute(d => (d.Minute % 15) != 0)
            .SkipYear(y => y < DateTime.Now.Year)

            .FormatMinute(d => $"{d:HH:mm}")
            .FormatText((dt, depth, b) => {
                b.PushL($"Select {depth}");
                b.PushL($"Current state: {dt}");
            })

            .Build(Message, new PagingService());

        await SendOrUpdate();
    }

    [Action]
    public async Task DT(string dt)
    {
        var datetime = DateTime.FromBinary(dt.Base64());
        Button("Select new", "/start");
        Push(datetime.ToString());
        await SendOrUpdate();
    }

    [On(Handle.Unknown)]
    public async Task Unknown()
    {
        Reply();
        await Send(Context!.GetSafeTextPayload()!);
    }

    [On(Handle.Exception)]
    public async Task Ex(Exception e)
    {
        Debugger.Break();
    }
}