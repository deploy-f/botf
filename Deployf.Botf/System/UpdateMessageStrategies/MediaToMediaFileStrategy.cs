using Telegram.Bot;
using Telegram.Bot.Types;

namespace Deployf.Botf.System.UpdateMessageStrategies;

/// <summary>
/// Situation: previous message has a media file and a new message has one.
/// </summary>
public class MediaToMediaFileStrategy : IUpdateMessageStrategy
{
    private readonly BotfBot _bot;

    public MediaToMediaFileStrategy(BotfBot bot)
    {
        _bot = bot;
    }
    
    public bool CanHandle(IUpdateMessageContext context)
    {
        var newMessageHasFile = context.MediaFile is InputMediaDocument;

        return context.PreviousMessage.Photo != null && newMessageHasFile;
    }

    public async Task<Message> UpdateMessage(IUpdateMessageContext context)
    {
        var updateMessagePolicy = context.UpdateContext.GetCurrentUpdateMsgPolicy();
        if (updateMessagePolicy is UpdateMessagePolicy.DeleteAndSend)
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
        else
        {
            return await _bot.Client.EditMessageMediaAsync(
                context.ChatId,
                context.MessageId,
                new InputMediaPhoto(context.MediaFile!)
                {
                    Caption = context.MessageText,
                    ParseMode = context.ParseMode
                },
                replyMarkup: context.KeyboardMarkup,
                cancellationToken: context.CancelToken
            );
        }
    }
}