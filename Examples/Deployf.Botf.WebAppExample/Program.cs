using Deployf.Botf;

BotfProgram.StartBot(args,
    onConfigure: (svc, conf) => {
        svc.AddRazorPages();
    },
    onRun: (app, conf) => {
        ((WebApplication)app).MapRazorPages();
    }
);

class WebAppExampleController : BotController
{
    readonly IConfiguration conf;

    public WebAppExampleController(IConfiguration conf)
    {
        this.conf = conf;
    }

    [Action("/start", "start the bot")]
    public async Task Start()
    {
        PushL($"Hello!");
        Push("This bot shows the example how to use webapps in BotF framework");

        // we can show webapp button in three ways
        // first: call next line and pass just only text argument
        // `text` is a text on a button
        // webapp url will be used from global configuration(botf key in the appsettings.*.json)
        Button(WebApp("Inline webapp"));

        // second: pass the webapp through second parameter in WebApp method
        Button(WebApp("Google as webapp", conf["MyWebAppUrl"]));

        await Send();

        Push("Or click on the keybord button below");

        // and third - in the keyboard
        KButton(KWebApp("Open webapp"));
    }

    [On(Handle.Unknown)]
    void OnUnknown()
    {
        var webappData = Context.Update?.Message?.WebAppData;
        if(webappData == null)
        {
            return;
        }

        PushL("Data from webapp: " + webappData.Data);
    }
}