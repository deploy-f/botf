using Telegram.Bot;
using Telegram.Bot.Types;

namespace Deployf.Botf.System.UpdateMessageStrategies;

/// <summary>
/// Situation: previous message has no media file, but a new message has one.
/// </summary>
public class PlainTextToMediaStrategy : IUpdateMessageStrategy
{
    private readonly BotfBot _bot;

    public PlainTextToMediaStrategy(BotfBot bot)
    {
        _bot = bot;
    }
    
    public bool CanHandle(IUpdateMessageContext context)
    {
        var newMessageHasFile = !string.IsNullOrEmpty(context.MediaFile?.FileId) ||
                                !string.IsNullOrEmpty(context.MediaFile?.Url);
        
        return context.PreviousMessage.Photo == null && newMessageHasFile;
    }

    public async Task<Message> UpdateMessage(IUpdateMessageContext context)
    {
        await _bot.Client.DeleteMessageAsync(context.ChatId, context.PreviousMessage.MessageId, context.CancelToken);
        return await _bot.Client.SendPhotoAsync(
            context.ChatId,
            context.MediaFile!,
            context.MessageText,
            context.ParseMode,
            replyMarkup: context.KeyboardMarkup,
            cancellationToken: context.CancelToken,
            replyToMessageId: context.ReplyToMessageId);
    }
}