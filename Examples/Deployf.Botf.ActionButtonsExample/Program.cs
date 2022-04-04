using Deployf.Botf;

BotfProgram.StartBot(args);

class ActionsController : BotController
{
    [Action("/start", "start")]
    void Start()
    {
        var guid = Guid.NewGuid();
        var q = Q(GuidArgumentButtonHandler, guid);
        Push($"callback: {q}");
        Button($"guid: {guid}", q);
    }
    
    [Action]
    async Task GuidArgumentButtonHandler(Guid guid)
    {
        Push($"value: {guid}");
        await Send();
        Start();
        await Send();
    }
}