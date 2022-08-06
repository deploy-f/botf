using Deployf.Botf;

BotfProgram.StartBot(args);

class ControllerStateExample : BotController
{
    [State]
    ExampleClass _data;

    [State]
    int intField;

    int nonStateIntField;

    [Action("/start", "start the bot")]
    public void Start()
    {
        PushL($"Hello!");
        PushL("This is an example of how to store controllers state(fields and props) through updates");
        PushL("Current controller state:");
        DumpState();

        PushL();
        PushL("For refresh call /start");

        RowButton("Set random _data", Q(SetRandom_data));
        RowButton("Set random intField", Q(SetRandom_intField));
        RowButton("Set random nonStateIntField", Q(SetRandom_nonStateIntField));
    }
    
    [Action]
    void SetRandom_data()
    {
        _data = new ExampleClass()
        {
            IntField = Random.Shared.Next(),
            StringProp = Guid.NewGuid().ToString()
        };
        Start();
    }

    [Action]
    void SetRandom_intField()
    {
        intField = Random.Shared.Next();
        Start();
    }

    [Action]
    void SetRandom_nonStateIntField()
    {
        nonStateIntField = Random.Shared.Next();
        Start();
    }

    void DumpState()
    {
        if(_data == null)
        {
            PushL("_data is null");
        }
        else
        {
            PushL($"_data is {_data.ToString()}");
        }

        PushL($"intField is {intField}");
        PushL($"nonStateIntField is {nonStateIntField}");
    }
}

class ExampleClass
{
    public int IntField;
    public string StringProp { get; set; }

    public override string ToString() =>  $"({IntField}, {StringProp})";
}