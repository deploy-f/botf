using Deployf.Botf;
using Deployf.Botf.Controllers;

class Program : BotfProgram
{
    public static void Main(string[] args) => StartBot(args);

    [Action("/start")]
    public async Task Start()
    {
        await Send($"Send `{nameof(Hello)}` to me, please!");
    }

    [Action]
    public async Task Hello()
    {
        await Send("Hey! Thank you! That's it.");
    }

    [On(Handle.Unknown)]
    public async Task Unknown()
    {
        PushL("You know.. it's very hard to recognize your command!");
        PushL("Please, write a correct text. Or use /start command");
        await Send();
    }
}