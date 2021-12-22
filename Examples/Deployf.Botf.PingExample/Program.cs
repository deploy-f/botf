using Deployf.Botf;

class Program : BotfProgram
{
    public static void Main(string[] args) => StartBot(args);

    [On(Handle.Unknown)]
    public async Task Unknown()
    {
        Reply();
        await Send(Context.GetSafeTextPayload()!);
    }
}

record State1;

class State1Controller : BotControllerState<State1>
{
}