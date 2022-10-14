using Deployf.Botf;

// This example shows how to send messages and update it.
class Program : BotfProgram
{
    public static void Main(string[] args) => StartBot(args);

    [Action("/start", "start the bot")]
    public void Start()
    {
        PushLL($"Hello!");
        PushL($"Auto send mesage feature: /autosend");
        PushL($"Simple send: /simplesend");
        PushL($"Send multiple messages in one action: /multisend");
        PushL($"Updating the message: /update");
    }

    [Action("/autosend")]
    public void AutoSend()
    {
        PushLL("All the text passed to Push* messages are buffered in controller's state and will be automatically send even without <code>Send()</code>");
        Push("If you don't want to use this feature just disable it through configuration <code>autosend</code> (should be like this: YourToken?<i>autosend=0</i>)");
    }

    [Action("/simplesend")]
    public async Task SimpleSend()
    {
        Push("Just a message sended though direct call to Send method");
        await Send(); // be сareful, Send is asynchronous
    }

    [Action("/multisend")]
    public async Task MultiSend()
    {
        PushL("Hello!");
        Push("Wait 1 second! I'm thinking...");
        await Send();

        // let's make delay between messages
        await Task.Delay(1000);

        Push("Thanks! I'm here!. But wait another second...");
        await Send();

        await Task.Delay(1000);

        Push("So this is my last message. Sorry, it's time to rest");
        
        // for last message you don't need to call `Send()` because autosend feature does it for you!
        // await Send()
    }

    [Action("/update")]
    public async Task UpdateInfo()
    {
        PushL("Let's play a game: change this message!");
        PushL("P.S. To change me use command /update with message id as first argument and new message text as second");
        PushL("Should be like: <code>/update 123 new_content</code>"); // There is a limitation: string parameters must be without space symbols. We know about that and will fix it in future!
        var message = await Send(); // Send and Update methods returns a message object - you can get its ID

        await Send($"Message is is <code>{message.MessageId}</code>");
    }

    [Action("/update")]
    public async Task UpdateMessage(int messageId, string newContent)
    {
        PushL("You change this message to:");
        Push(newContent); // Update all the content

        MessageId = messageId; // here we tell to BotF that we want to update a certain message
        await Update();
    }

    // in this method we catch all exceptions that causes in user's controllers
    // firstly - to show error if you send wrong messageId
    [On(Handle.Exception)]
    void ExceptionHandler(Exception e)
    {
        Push("Unhandled exception: ");
        Push(e.GetType().Name + ": ");
        Push(e.Message);
    }
}