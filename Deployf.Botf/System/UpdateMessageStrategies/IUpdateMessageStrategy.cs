using Telegram.Bot.Types;

namespace Deployf.Botf.System.UpdateMessageStrategies;

public interface IUpdateMessageStrategy
{
    public bool CanHandle(IUpdateMessageContext context);
    public Task<Message> UpdateMessage(IUpdateMessageContext context);
}