using Telegram.Bot;
using Telegram.Bot.Types;

namespace Deployf.Botf.System.UpdateMessageStrategies;

/// <summary>
/// Situation: previous message has media file, but a new message does not have.
/// </summary>
public class FileStrategy1 : IUpdateMessageStrategy
{
    private readonly BotfBot _bot;

    public FileStrategy1(BotfBot bot)
    {
        _bot = bot;
    }
    
    public bool CanHandle(IUpdateMessageContext context)
    {
        var newMessageFileIsEmpty = string.IsNullOrEmpty(context.MediaFile?.FileId) &&
                                    string.IsNullOrEmpty(context.MediaFile?.Url);
        
        return context.PreviousMessage.Photo != null && newMessageFileIsEmpty;
    }

    public async Task<Message> UpdateMessage(IUpdateMessageContext context)
    {
        await _bot.Client.DeleteMessageAsync(context.ChatId, context.PreviousMessage.MessageId, context.CancelToken);
        return await _bot.Client.SendTextMessageAsync(
            context.ChatId,
            context.MessageText,
            context.ParseMode,
            replyMarkup: context.KeyboardMarkup,
            cancellationToken: context.CancelToken,
            replyToMessageId: context.ReplyToMessageId);
    }
}