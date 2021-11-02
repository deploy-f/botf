using Telegram.Bot;
using Telegram.Bot.Framework;

namespace Deployf.Botf.Extensions
{
    public class BotfBot : BotBase
    {
        public BotfBot(BotfOptions options) : base(options.Username, new TelegramBotClient(options.Token)){}
    }
}