using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public interface IBotContextAccessor
{
    IUpdateContext? Context { get; set; }
}