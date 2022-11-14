using Deployf.Botf;
using Telegram.Bot.Framework.Abstractions;

// This example shows how to handle unknown type of updates.
// Unknown means all updates that is not handled by any of [Action("...")] attribute and other Handle's type.
class Program : BotfProgram
{
    public static void Main(string[] args) => StartBot(args);

    [Action("/start", "start the bot")]
    public void Start()
    {
        Push($"Send any kind of message to me, please!");
        Button("Callback test", "data");
    }

    // The handler processes only unhandled clicks to the buttons
    [On(Handle.Unknown)]
    [Filter(Filters.CallbackQuery)]
    public void UnknownCallback()
    {
        PushL("Unknown callback");
        // StopHandling tells to the BotF that if there are other handlers, BotF must not call them after processing current handler.
        Context.StopHandling();
    }

    // This handler processes only unknown commands
    // third argument tells to the BotF that this handler has `1` order in the queue of processing.
    // The higher the order -> the faster it will be processed
    // for example:
    // | order      | handler             |
    // +------------+---------------------+
    // | 10         | Handler10           |
    // | 1          | Handler1            |
    // | 0(default) | DefaultHandler      |
    // | -1         | HandlerUnderDefault |
    // 
    // Handler10(with 10 order) will be processed first, then Handler1, then DefaultHandler(without specefic order) and HandlerUnderDefault will be processed last
    [On(Handle.Unknown, 1)]
    [Filter(Filters.Command)]
    public void UnknownCommand()
    {
        Reply();
        PushL("Unknown command");
        Context.StopHandling();
    }

    // This handler has custom filter function. It means that you can implement your own filter function(see next method)
    // To provide the filter-function into the Botf you can tell the name of the function.
    // You can use just identifier of the function inside the same class as the handler,
    // or provide full path to the method, with namespace and declared class (like YourFancyNamespace.YourClass.FilterFunctionName)
    [On(Handle.Unknown, 1)]
    [Filter(nameof(Filter))]
    public void UnknownCustomFilter()
    {
        Reply();
        PushL("Symbol is found");
        Context.StopHandling();
    }

    // Filter-function must be:
    // * static
    // * return bool value
    // * receive single argument with IUpdateContext type
    public static bool Filter(IUpdateContext ctx)
    {
        var message = ctx.Update.Message?.Text;
        if(string.IsNullOrEmpty(message))
        {
            return false;
        }

        return message.Contains("filter");
    }
    
    [On(Handle.Unknown)]
    [Filter(Filters.Contact)]
    public void UnknownContactHandler()
    {
        Reply();
        PushL("Thank you, I will add you to my contact list");
        Context.StopHandling();
    }
    
    // This handler will process all new text messages.
    // But if previus handler has called `Context.StopHandling()` it will not be handled.
    [On(Handle.Unknown)]
    [Filter(Filters.Text)]
    public void UnknownNewTextHandler()
    {
        Reply();
        PushL("Unknown text message");
        Context.StopHandling();
    }

    // This handler catch all messages that contains text pattern like "Hello ***!"
    // And reply for it with "Hey!"
    [On(Handle.Unknown, 5)]
    [Filter(Filters.Text)]
    [Filter(And: Filters.Regex, Param: "Hello .*!")]
    public void UnknownTextHello()
    {
        Reply();
        PushL("Hey!");
        Context.StopHandling();
    }

    // This handler will handle all other unhandled updates that was not handled yet. It's called as "general" handler
    [On(Handle.Unknown)]
    public void UnknownGeneralHandler()
    {
        Reply();
        PushL("Geleral Handler");
    }
}