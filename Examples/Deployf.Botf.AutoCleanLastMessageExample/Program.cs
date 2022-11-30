using Deployf.Botf;

BotfProgram.StartBot(args);

class ExampleController : BotController
{
    [Action("/start", "start the bot")]
    public void Start()
    {
        PushL("Hello!");
        PushLL("This example shows the auto-clean feature of BotF");
        
        PushL("If you send me a message this message will be deleted automatically.");
        PushL("But make sure that you have `keep_clean=true` in the connection string.");
    }

    [On(Handle.Unknown)]
    public void Unknown()
    {
        PushL("You see!");
        PushL("I deleted my last message without any line of code in the Program.cs!");
        Button("If you click on this button I will not delete it", Q(Callback));
    }
    
    [Action]
    public void Callback()
    {
        PushL("Message was updated");
        Push("Send me /many please");
    }
    
    [Action("/many", "example of many messages")]
    public async Task Many()
    {
        DontSaveMessageId = true;
        await Send("First message");
        Push("Second message. I will delete only this one. ");
        Push("First message will not be deleted. If you want to see how to delete them all send me /many_delete");
        DontSaveMessageId = false;
    }
}