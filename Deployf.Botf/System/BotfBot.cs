using Telegram.Bot;
using Telegram.Bot.Framework;

namespace Deployf.Botf;

public class BotfBot : BotBase
{
    public readonly BotfOptions Options;

    public BotfBot(BotfOptions options)
        : base(options.Username, new TelegramBotClient(new TelegramBotClientOptions(options.Token!, baseUrl: options.ApiBaseUrl)))
    {
        Options = options;
    }
}