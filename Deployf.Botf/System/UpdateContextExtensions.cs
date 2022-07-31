using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace Deployf.Botf;

public static class UpdateContextExtensions
{
    private const string STOP_HANDLING_KEY = "$_StopHandling";
    private static readonly object _handlingStopMarkerItem = new object();

    public static long GetChatId(this IUpdateContext context)
    {
        return context.Update!.Message!.Chat.Id;
    }

    public static long GetUserId(this IUpdateContext context)
    {
        return context!.Update!.Message!.From!.Id;
    }

    public static long? GetSafeChatId(this IUpdateContext context)
    {
        return context.Update.Message?.Chat.Id
               ?? context.Update.EditedMessage?.Chat.Id
               ?? context.Update.CallbackQuery?.Message?.Chat.Id
               ?? context.Update.InlineQuery?.From.Id;
    }

    public static long? GetSafeUserId(this IUpdateContext context)
    {
        return context.Update.Message?.From?.Id
               ?? context.Update.EditedMessage?.From?.Id
               ?? context.Update.CallbackQuery?.From?.Id
               ?? context.Update.InlineQuery?.From.Id;
    }

    public static long UserId(this IUpdateContext context)
    {
        var value = context.Update.Message?.From?.Id
               ?? context.Update.EditedMessage?.From?.Id
               ?? context.Update.CallbackQuery?.From?.Id
               ?? context.Update.InlineQuery?.From?.Id;

        return value!.Value;
    }

    public static int? GetMessageId(this IUpdateContext context)
    {
        return context.Update.Message?.MessageId;
    }

    public static int? GetCallbackMessageId(this IUpdateContext context)
    {
        return context.Update!.CallbackQuery!.Message?.MessageId;
    }

    public static int? GetSafeMessageId(this IUpdateContext context)
    {
        return context.Update.Message?.MessageId
               ?? context.Update.CallbackQuery?.Message?.MessageId
               ?? context.Update.EditedMessage?.MessageId;
    }

    public static string? GetSafeTextPayload(this IUpdateContext context)
    {
        return context.Update.Message?.Text
               ?? context.Update.CallbackQuery?.Data
               ?? context.Update.InlineQuery?.Query;
    }

    public static CallbackQuery GetCallbackQuery(this IUpdateContext context)
    {
        return context!.Update!.CallbackQuery!;
    }

    public static long GetCallbackQueryChatId(this IUpdateContext context)
    {
        return context!.Update!.CallbackQuery!.Message!.Chat.Id;
    }

    public static string? GetTypeValue(this IUpdateContext context)
    {
        return context.Update.Message?.Text
               ?? context.Update.CallbackQuery?.Data
               ?? context.Update.EditedMessage?.Text;
    }

    public static string? GetUsername(this IUpdateContext context)
    {
        return context.Update.Message?.From?.Username
               ?? context.Update.CallbackQuery?.From.Username
               ?? context.Update.EditedMessage?.From?.Username;
    }

    public static string GetUserFullName(this IUpdateContext context)
    {
        var first = context.Update.Message?.From?.FirstName
               ?? context.Update.CallbackQuery?.From?.FirstName
               ?? context.Update.EditedMessage?.From?.FirstName;

        var last = context.Update.Message?.From?.LastName
               ?? context.Update.CallbackQuery?.From?.LastName
               ?? context.Update.EditedMessage?.From?.LastName;

        return first + " " + last;
    }

    public static void StopHandling(this IUpdateContext context)
    {
        context.Items[STOP_HANDLING_KEY] = _handlingStopMarkerItem;
    }
    public static bool IsHandlingStopRequested(this IUpdateContext context)
    {
        return context.Items.ContainsKey(STOP_HANDLING_KEY);
    }
}