using Telegram.Bot;
using Telegram.Bot.Types;

namespace Deployf.Botf.System.UpdateMessageStrategies;

/// <summary>
/// Situation: previous message has media file, but a new message does not have.
/// </summary>
public class MediaToPlainTextStrategy : IUpdateMessageStrategy
{
    private readonly BotfBot _bot;

    public MediaToPlainTextStrategy(BotfBot bot)
    {
        _bot = bot;
    }
    
    public bool CanHandle(IUpdateMessageContext context)
    {
        var newMessageFileIsEmpty = context.MediaFile is InputMediaDocument;

        return context.PreviousMessage.Photo != null && newMessageFileIsEmpty;
    }

    public async Task<Message> UpdateMessage(IUpdateMessageContext context)
    {
        await _bot.Client.DeleteMessageAsync(context.ChatId, context.PreviousMessage.MessageId, context.CancelToken);
        return await _bot.Client.SendTextMessageAsync(
            context.ChatId,
            context.MessageText,
            null,
            context.ParseMode,
            replyMarkup: context.KeyboardMarkup,
            cancellationToken: context.CancelToken,
            replyToMessageId: context.ReplyToMessageId);
    }
}