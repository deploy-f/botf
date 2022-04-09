using Telegram.Bot;
using Telegram.Bot.Types;

namespace Deployf.Botf;

public class MessageSender
{
    readonly ITelegramBotClient _client;

    public MessageSender(ITelegramBotClient client)
    {
        _client = client;
    }

    // TODO: to catch api exceptions about "forbidden"
    public async ValueTask<Message> Send(MessageBuilder message, CancellationToken token = default)
    {
        if (message.PhotoUrl == null)
        {
            return await _client.SendTextMessageAsync(
                message.ChatId,
                message.Message,
                message.ParseMode,
                replyMarkup: message.Markup,
                cancellationToken: token,
                replyToMessageId: message.ReplyToMessageId
            );
        }
        else
        {
            return await _client.SendPhotoAsync(
                message.ChatId,
                message.PhotoUrl,
                message.Message,
                message.ParseMode,
                replyMarkup: message.Markup,
                cancellationToken: token,
                replyToMessageId: message.ReplyToMessageId
            );
        }
    }
}
