using Deployf.Botf;
using Telegram.Bot.Types.Enums;

#if NET5_0
    using ValueTask = System.Threading.Tasks.ValueTask;
    using ValueTaskGeneric = System.Threading.Tasks.ValueTask<object>;
#else
using ValueTask = System.Threading.Tasks.Task;
using ValueTaskGeneric = System.Threading.Tasks.Task<object>;
#endif

class Program : BotfProgram
{
    public static void Main(string[] args) => StartBot(args);

    readonly ILogger<Program> _logger;

    public Program(ILogger<Program> logger)
    {
        _logger = logger;
    }

    [Action("Start")]
    [Action("/start", "start the bot")]
    public void Start()
    {
        Push($"Send `{nameof(Hello)}` to me, please!");

        RowKButton(Q(State1Go));
        RowKButton(Q(Hello));
        RowKButton(Q(Check));
    }

    [Action("State 1")]
    public async Task State1Go()
    {
        await Send($"Going to state 1");
        await GlobalState(new State1());
    }

    [Action(nameof(Hello))]
    public void Hello()
    {
        Push("Hey! Thank you! That's it.");
    }

    [Action("Check")]
    public void Check()
    {
        Push($"This is main state");
    }

    [On(Handle.Unknown)]
    public async Task Unknown()
    {
        PushL("unknown");
        await Send();
    }

    [On(Handle.ClearState)]
    public void ClearState()
    {
        RowKButton(Q(State1Go));
        RowKButton(Q(Hello));
        RowKButton(Q(Check));

        Push("Main menu");
    }

    [On(Handle.Exception)]
    public async Task Ex(Exception e)
    {
        _logger.LogCritical(e, "Unhandled exception");

        if (Context.Update.Type == UpdateType.CallbackQuery)
        {
            await AnswerCallback("Error");
        }
        else if (Context.Update.Type == UpdateType.Message)
        {
            Push("Error");
        }
    }
}

record State1;
record State2;

class State1Controller : BotControllerState<State1>
{
    public override async ValueTask OnEnter()
    {
        RowKButton(Q(GoToMain));
        RowKButton(Q(GoToState2));
        RowKButton(Q(Check));

        await Send("Enter State1");
    }

    public override async ValueTask OnLeave()
    {
        await Send("Leave State1");
    }

    [Action("Main")]
    public async Task GoToMain()
    {
        Push($"Going to main state");
        await GlobalState(null);
    }

    [Action("State 2")]
    public async Task GoToState2()
    {
        Push($"Going to state 2");
        await GlobalState(new State2());
    }

    [Action("Check")]
    public void Check()
    {
        Push($"This is state 1");
    }
}

class State2Controller : BotControllerState<State2>
{
    public override async ValueTask OnEnter()
    {
        RowKButton(Q(GoToMain));
        RowKButton(Q(GoToState1));
        RowKButton(Q(Check));

        await Send("Enter State2");
    }

    public override async ValueTask OnLeave()
    {
        await Send("Leave State2");
    }

    [Action("Main")]
    public async Task GoToMain()
    {
        Push($"Going to main state");
        await GlobalState(null);
    }

    [Action("State 1")]
    public async Task GoToState1()
    {
        Push($"Going to state 1");
        await GlobalState(new State1());
    }

    [Action("Check")]
    public void Check()
    {
        Push($"This is state 2");
    }
}