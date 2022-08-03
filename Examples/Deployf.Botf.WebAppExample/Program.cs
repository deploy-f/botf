using Deployf.Botf;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Framework;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

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

        //enable inline mode for this bot in @BotFather
        RowButton(InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Search", "Happy "));

        await Send();

        Push("Or click on the keybord button below");

        // and third - in the keyboard
        KButton(KWebApp("Open webapp"));
    }

    [On(Handle.Unknown)]
    async Task OnUnknown()
    {
        //var webappData = Context.Update?.Message?.WebAppData;
        //if(webappData == null)
        //{
        //    return;
        //}

        //PushL("Data from webapp: " + webappData.Data);
        switch (Context.Update?.Type)
        {
            case UpdateType.Message:
                var webappData = Context.Update?.Message?.WebAppData;
                if (webappData != null)
                {
                    PushL("Data from page: " + webappData.Data);
                }
                break;
            case UpdateType.InlineQuery:
                var query = Context.Update?.InlineQuery?.Query;
                if (query != null)
                {
                    // set bot name
                    var botName = conf["BotUserName"];
                    // Some variables
                    string link = $"https://t.me/{botName}";
                    // Your Button that you want
                    InlineKeyboardButton button = InlineKeyboardButton.WithUrl("Go to postcards bot", link);
                    var id = Context.Update?.InlineQuery?.Id;
                    // Inline Results two images
                    InlineQueryResult[] results = {
                            new InlineQueryResultPhoto(1.ToString(),"https://images5.alphacoders.com/813/813527.jpg","https://images2.alphacoders.com/813/thumb-1920-813527.jpg")
                            {
                                Caption = "Some caption 1",
                                Description = "Come description 1",
                                Title = "Some title 1",
                                ReplyMarkup = new InlineKeyboardMarkup(button)
                            },
                            new InlineQueryResultPhoto(2.ToString(),"https://images2.alphacoders.com/967/96730.jpg","https://images2.alphacoders.com/967/thumb-1920-96730.jpg")
                            {
                                Caption = "Some caption 2",
                                Description = "Come description 2",
                                Title = "Some title 2",
                                ReplyMarkup = new InlineKeyboardMarkup(button)
                            },
                    };

                    // Answer with results:
                    await Client.AnswerInlineQueryAsync(id, results, isPersonal: true, cacheTime: 0);
                }
                break;
        }

        //PushL("Data from webapp: " + webappData.Data);
    }
}