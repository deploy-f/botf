using Deployf.Botf;

class Program : BotfProgram
{
    // It's boilerplate program entrypoint.
    // We just simplified all usual code into static method StartBot.
    // But in this case of starting of the bot, you should add a config section under "bot" key to appsettings.json
    public static void Main(string[] args) => StartBot(args);

    // Action attribute mean that you mark async method `Start`
    // as handler for user's text in message which equal to '/start' string.
    // You can name method as you want
    // And also, second argument of Action's attribute is a description for telegram's menu for this action
    [Action("/start", "start the bot")]
    public void Start()
    {
        // Just sending a reply message to user. Very simple, isn't?
        Push($"Send `{nameof(Hello)}` to me, please!");
    }

    // If we dont put any parameter into Action attribute,
    // it means that this method will handle messages with hame of the method.
    // Yep, in this case, you should care about the method's name.
    [Action]
    public void Hello()
    {
        Push("Hey! Thank you! That's it.");
    }

    // Here we handle all unknown command or just text sent from user
    [On(Handle.Unknown)]
    public async Task Unknown()
    {
        // Here, we use the so-called "buffering of sending message"
        // It means you dont need to construct all message in the string and send it once
        // You can use Push to just add the text to result message, or PushL - the same but with new line after the string.
        PushL("You know.. it's very hard to recognize your command!");
        PushL("Please, write a correct text. Or use /start command");

        // And finally, send buffered message
        await Send();
    }
}