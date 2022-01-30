using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class BotContextAccessor : IBotContextAccessor
{
    public IUpdateContext? Context { get; set; }
}