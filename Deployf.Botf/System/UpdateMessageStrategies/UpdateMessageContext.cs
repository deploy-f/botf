using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Deployf.Botf.System.UpdateMessageStrategies;

public interface IUpdateMessageContext
{
    public IUpdateContext UpdateContext { get; init; }
    public long ChatId { get; init; }
    public int MessageId { get; init; }
    public string MessageText { get; init; }
    public Message PreviousMessage { get; init; }
    public InputMedia? MediaFile { get; init; }
    public InlineKeyboardMarkup? KeyboardMarkup { get; init; }
    public ParseMode ParseMode { get; init; }
    public int ReplyToMessageId { get; init; }
    public CancellationToken CancelToken { get; init; }
} 

public record UpdateMessageContext(IUpdateContext UpdateContext, long ChatId, int MessageId, string MessageText, Message PreviousMessage,
    InputMedia? MediaFile, InlineKeyboardMarkup? KeyboardMarkup, ParseMode ParseMode, int ReplyToMessageId, 
    CancellationToken CancelToken) : IUpdateMessageContext;