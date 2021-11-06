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
        PushL("Select year");

        DateTimePicker(state, Q(YearSelect, "{0}"), d => Q(DT, d.ToBinary().Base64()));

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

    private void DateTimePicker(string state, string nav, Func<DateTime, string> click)
    {
        new CalendarMessageBuilder()
            .Year(DateTime.Now.Year)
            .Month(DateTime.Now.Month)
            .Depth(CalendarDepth.Minutes)
            .SetState(state)
            .OnNavigatePath(state => nav.Replace("{0}", state))
            .OnSelectPath(click)
            .Build(Message, pagingService);
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