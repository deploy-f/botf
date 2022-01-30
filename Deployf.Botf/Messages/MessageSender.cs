using Telegram.Bot;

namespace Deployf.Botf;

public class MessageSender
{
    readonly ITelegramBotClient _client;

    public MessageSender(ITelegramBotClient client)
    {
        _client = client;
    }

    // TODO: to catch api exceptions about "forbidden"
    public async ValueTask Send(MessageBuilder message, CancellationToken token = default)
    {
        await _client.SendTextMessageAsync(
            message.ChatId,
            message.Message,
            message.ParseMode,
            replyMarkup: message.Markup,
            cancellationToken: token,
            replyToMessageId: message.ReplyToMessageId
        );
    }
}
