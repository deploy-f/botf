using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace Deployf.Botf;

public static class UpdateContextExtensions
{
    private const string UPDATE_MESSAGE_POLICY_KEY = "$_UpdateMessagePolicy";
    private const string STOP_HANDLING_KEY = "$_StopHandling";
    private const string CURRENT_HANDLER_KEY = "$_CurrentHandler";
    private const string FILTER_PARAMETER_KEY = "$_FilterParameter";
    private static readonly object _handlingStopMarkerItem = new object();

    public static long GetChatId(this IUpdateContext context)
    {
        if (context.ChatId.HasValue)
        {
            return context.ChatId.Value;
        }
        
        return context.Update!.Message!.Chat.Id;
    }

    public static long GetUserId(this IUpdateContext context)
    {
        return context!.Update!.Message!.From!.Id;
    }

    public static long? GetSafeChatId(this IUpdateContext context)
    {
        if (context.ChatId.HasValue)
        {
            return context.ChatId.Value;
        }

        return context.Update.Message?.Chat.Id
               ?? context.Update.EditedMessage?.Chat.Id
               ?? context.Update.ChannelPost?.Chat.Id
               ?? context.Update.EditedChannelPost?.Chat.Id
               ?? context.Update.CallbackQuery?.Message?.Chat.Id
               ?? context.Update.MyChatMember?.Chat.Id
               ?? context.Update.ChatMember?.Chat.Id
               ?? context.Update.ChatJoinRequest?.Chat.Id;
    }

    public static Chat? GetSafeChat(this IUpdateContext context)
    {
        return context.Update.Message?.Chat
               ?? context.Update.EditedMessage?.Chat
               ?? context.Update.ChannelPost?.Chat
               ?? context.Update.EditedChannelPost?.Chat
               ?? context.Update.CallbackQuery?.Message?.Chat
               ?? context.Update.MyChatMember?.Chat
               ?? context.Update.ChatMember?.Chat
               ?? context.Update.ChatJoinRequest?.Chat;
    }

    public static long? GetSafeUserId(this IUpdateContext context)
    {
        if (context.UserId.HasValue)
        {
            return context.UserId.Value;
        }
        return context.Update.Message?.From?.Id
               ?? context.Update.EditedMessage?.From?.Id
               ?? context.Update.ChannelPost?.From?.Id
               ?? context.Update.EditedChannelPost?.From?.Id
               ?? context.Update.InlineQuery?.From.Id
               ?? context.Update.ChosenInlineResult?.From.Id
               ?? context.Update.CallbackQuery?.From?.Id
               ?? context.Update.ShippingQuery?.From.Id
               ?? context.Update.PreCheckoutQuery?.From.Id
               ?? context.Update.PollAnswer?.User.Id
               ?? context.Update.MyChatMember?.From.Id
               ?? context.Update.ChatMember?.From.Id
               ?? context.Update.ChatJoinRequest?.From.Id;
    }
    
    public static User? GetSafeUser(this IUpdateContext context)
    {
        return context.Update.Message?.From
               ?? context.Update.EditedMessage?.From
               ?? context.Update.ChannelPost?.From
               ?? context.Update.EditedChannelPost?.From
               ?? context.Update.InlineQuery?.From
               ?? context.Update.ChosenInlineResult?.From
               ?? context.Update.CallbackQuery?.From
               ?? context.Update.ShippingQuery?.From
               ?? context.Update.PreCheckoutQuery?.From
               ?? context.Update.PollAnswer?.User
               ?? context.Update.MyChatMember?.From
               ?? context.Update.ChatMember?.From
               ?? context.Update.ChatJoinRequest?.From;
    }

    public static long UserId(this IUpdateContext context)
    {
        if (context.UserId.HasValue)
        {
            return context.UserId.Value;
        }
        
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
    
    public static InlineQuery GetInlineQuery(this IUpdateContext context)
    {
        return context!.Update!.InlineQuery!;
    }
    
    public static string GetInlineQueryId(this IUpdateContext context)
    {
        return context!.Update!.InlineQuery!.Id;
    }
    
    public static string? GetSafeInlineQueryId(this IUpdateContext context)
    {
        return context!.Update!.InlineQuery?.Id;
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
    public static string? GetLangCode(this IUpdateContext context)
    {
        var value = context.Update.Message?.From?.LanguageCode
                    ?? context.Update.EditedMessage?.From?.LanguageCode
                    ?? context.Update.CallbackQuery?.From?.LanguageCode
                    ?? context.Update.InlineQuery?.From?.LanguageCode;

        return value;
    }

    public static void StopHandling(this IUpdateContext context)
    {
        context.Items[STOP_HANDLING_KEY] = _handlingStopMarkerItem;
    }
    public static bool IsHandlingStopRequested(this IUpdateContext context)
    {
        return context.Items.ContainsKey(STOP_HANDLING_KEY);
    }

    public static void SetCurrentHandler(this IUpdateContext context, HandlerItem? handler)
    {
        if(handler == null && context.Items.ContainsKey(CURRENT_HANDLER_KEY))
        {
            context.Items.Remove(CURRENT_HANDLER_KEY);
        }
        else if(handler != null)
        {
            context.Items[CURRENT_HANDLER_KEY] = handler;
        }
    }
    public static HandlerItem? GetCurrentHandler(this IUpdateContext context)
    {
        if(context.Items.TryGetValue(CURRENT_HANDLER_KEY, out var handler) && handler != null && handler is HandlerItem result)
        {
            return result;
        }

        return null;
    }

    public static void SetFilterParameter(this IUpdateContext context, object? parameter)
    {
        if(parameter == null && context.Items.ContainsKey(FILTER_PARAMETER_KEY))
        {
            context.Items.Remove(FILTER_PARAMETER_KEY);
        }
        else if(parameter != null)
        {
            context.Items[FILTER_PARAMETER_KEY] = parameter;
        }
    }
    public static object? GetFilterParameter(this IUpdateContext context)
    {
        if(context.Items.TryGetValue(FILTER_PARAMETER_KEY, out var parameter))
        {
            return parameter;
        }

        return null;
    }
    
    public static void SetUpdateMsgPolicy(this IUpdateContext context, UpdateMessagePolicy policy)
    {
        context.Items[UPDATE_MESSAGE_POLICY_KEY] = policy;
    }
    
    public static UpdateMessagePolicy? GetCurrentUpdateMsgPolicy(this IUpdateContext context)
    {
        if(context.Items.TryGetValue(UPDATE_MESSAGE_POLICY_KEY, out var policy) && policy is UpdateMessagePolicy result)
        {
            return result;
        }

        return null;
    }
}

public enum UpdateMessagePolicy
{
    UpdateContent = 0, // default
    DeleteAndSend = 1
}