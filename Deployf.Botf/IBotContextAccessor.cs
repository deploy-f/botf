using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf.Services
{
    public interface IBotContextAccessor
    {
        IUpdateContext Context { get; set; }
    }

    public class BotContextAccessor : IBotContextAccessor
    {
        public IUpdateContext Context { get; set; }
    }
}