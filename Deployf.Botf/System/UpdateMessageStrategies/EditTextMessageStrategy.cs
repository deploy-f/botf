using Telegram.Bot;
using Telegram.Bot.Types;

namespace Deployf.Botf.System.UpdateMessageStrategies;

/// <summary>
/// Situation: previous message has no media file and a new message does not have.
/// </summary>
public class EditTextMessageStrategy : IUpdateMessageStrategy
{
    private readonly BotfBot _bot;

    public EditTextMessageStrategy(BotfBot bot)
    {
        _bot = bot;
    }
    
    public bool CanHandle(IUpdateMessageContext context)
    {
        var newMessageFileIsEmpty = context.MediaFile is InputMediaDocument;

        return context.PreviousMessage.Photo == null && newMessageFileIsEmpty;
    }

    public async Task<Message> UpdateMessage(IUpdateMessageContext context)
    {
        return await _bot.Client.EditMessageTextAsync(
            context.ChatId,
            context.MessageId,
            context.MessageText,
            parseMode: context.ParseMode,
            replyMarkup: context.KeyboardMarkup,
            cancellationToken: context.CancelToken
        );
    }
}