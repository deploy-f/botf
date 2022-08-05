using Deployf.Botf;

BotfProgram.StartBot(args);

class ActionAndQueryController : BotController
{
    [Action("/start", "start the bot")]
    public void Start()
    {
        PushL($"Hello!");
        Push("This an example of how to use Q(...) and action's parameters");

        RowButton("Simple action", Q(ActionWithNoArgs));
        RowButton("Action with primitive args", Q(ActionWithPrimitiveArgs, 10, "hi"));

        var instance = new ExampleClass
        {
            IntField = 25,
            StringProp = "very looooong string with many words"
        };
        RowButton("Action with class", Q(ActionWithStoredValue, instance));
    }
    
    [Action]
    void ActionWithNoArgs()
    {
        Push("Just action :)");

        RowButton("Back", Q(Start));
        RowButton("Back(manually)", "/start");
    }

    [Action]
    void ActionWithPrimitiveArgs(int arg1, string arg2)
    {
        PushL("Action with primitive arguments");
        PushL($"Arg1: {arg1}");
        PushL($"Arg2: {arg2}");

        RowButton("Back", Q(Start));
    }

    [Action]
    void ActionWithStoredValue(ExampleClass instance)
    {
        PushL("Action with class as a parameter");
        PushL($"IntField: {instance.IntField}");
        PushL($"StringProp: {instance.StringProp}");

        instance.IntField += 1;
        var action = Q(ActionWithStoredValue, instance);
        RowButton("IntField += 1", action);

        RowButton("Back", Q(Start));
    }
}

class ExampleClass
{
    public int IntField;
    public string StringProp { get; set; }
}