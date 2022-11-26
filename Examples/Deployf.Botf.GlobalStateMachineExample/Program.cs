using Deployf.Botf;
using Telegram.Bot.Types.Enums;

class Program : BotfProgram
{
    private static IServiceProvider RootProvider;
    public static void Main(string[] args) => StartBot(args, onRun: (app, conf) =>
    {
        // need to have root provider to create scope for SetStateDeffered example
        RootProvider = app.ApplicationServices;
    });

    readonly ILogger<Program> _logger;
    readonly IGlobalStateService _globalState;
    public Program(ILogger<Program> logger, IGlobalStateService globalState)
    {
        _logger = logger;
        _globalState = globalState;
    }

    [Action("Start")]
    [Action("/start", "start the bot")]
    public void Start()
    {
        Push($"Send `{nameof(Hello)}` to me, please!");

        RowKButton(Q(State1Go));
        RowKButton(Q(Hello));
        RowKButton(Q(Check));
        RowKButton(Q(SetStateDeffered));
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
    
    [Action("Deffered")]
    public void SetStateDeffered()
    {
        Push($"I will set state in 2 sec for {FromId} to {nameof(State2)}");

        var _ = SetDeffered(FromId);

        async Task SetDeffered(long userId)
        {
            await Task.Delay(2000);

            var scope = RootProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IGlobalStateService>();
            await service.SetState(userId, new State2());
        }
    }
    
    [Action("/deffered")]
    public async Task SetStateDefferedForUserId(long userId)
    {
        await _globalState.SetState(userId, new State2());
    }

    [On(Handle.Unknown)]
    public async Task Unknown()
    {
        PushL("unknown");
        await Send();
    }

    [On(Handle.ClearState)]
    public void CleanState()
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

    [On(Handle.Unknown, 1)]
    [Filter(Filters.CurrentGlobalState)]
    void UnknownForThisState()
    {
        Reply();
        Push("Unknown command for State1");
        Context.StopHandling();
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