using System.Text.RegularExpressions;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;

namespace Deployf.Botf;

public static class Filters
{
    private const string _BASE = $"{nameof(Deployf)}.{nameof(Botf)}.{nameof(FiltersImpl)}";
    public const string Text = $"{_BASE}.{nameof(FiltersImpl.FilterNewTextMessage)}";
    public const string Messages = $"{_BASE}.{nameof(FiltersImpl.FilterAllMessages)}";
    public const string CallbackQuery = $"{_BASE}.{nameof(FiltersImpl.FilterCallbackQuery)}";
    public const string NotGlobalState = $"{_BASE}.{nameof(FiltersImpl.FilterNotGlobalState)}";
    public const string CurrentGlobalState = $"{_BASE}.{nameof(FiltersImpl.FilterCurrentGlobalState)}";
    public const string InlineQuery = $"{_BASE}.{nameof(FiltersImpl.FilterInlineQuery)}";
    public const string Command = $"{_BASE}.{nameof(FiltersImpl.FilterCommands)}";
    public const string Media = $"{_BASE}.{nameof(FiltersImpl.FilterMedia)}";
    public const string Document = $"{_BASE}.{nameof(FiltersImpl.FilterDocument)}";
    public const string Photo = $"{_BASE}.{nameof(FiltersImpl.FilterPhoto)}";
    public const string Audio = $"{_BASE}.{nameof(FiltersImpl.FilterAudio)}";
    public const string Sticker = $"{_BASE}.{nameof(FiltersImpl.FilterSticker)}";
    public const string Regex = $"{_BASE}.{nameof(FiltersImpl.FilterRegex)}";
    public const string PrivateChat = $"{_BASE}.{nameof(FiltersImpl.FilterPrivateChats)}";
    public const string GroupChat = $"{_BASE}.{nameof(FiltersImpl.FilterGroupChats)}";
    public const string Type = $"{_BASE}.{nameof(FiltersImpl.FilterType)}";
}

public static class FiltersImpl
{
    public static bool FilterAllMessages(IUpdateContext ctx)
    {
        var update = ctx.Update;

        if(update.Type == UpdateType.EditedMessage || update.Type == UpdateType.Message)
        {
            return true;
        }

        return false;
    }

    public static bool FilterNewTextMessage(IUpdateContext ctx)
    {
        var update = ctx.Update;

        return update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text;
    }

    public static bool FilterPrivateChats(IUpdateContext ctx)
    {
        var update = ctx.Update;

        var chat = update?.Message?.Chat
            ?? update?.EditedMessage?.Chat
            ?? update?.CallbackQuery?.Message?.Chat
            ?? update?.MyChatMember?.Chat;
        
        return chat != null && chat.Type == ChatType.Private;
    }

    public static bool FilterGroupChats(IUpdateContext ctx)
    {
        var update = ctx.Update;

        var chat = update?.Message?.Chat
            ?? update?.EditedMessage?.Chat
            ?? update?.CallbackQuery?.Message?.Chat
            ?? update?.MyChatMember?.Chat;
        
        return chat != null && chat.Type == ChatType.Group;
    }

    public static bool FilterType(IUpdateContext ctx)
    {
        var parameter = ctx.GetFilterParameter();

        if(parameter == null || parameter is not UpdateType type)
        {
            throw new BotfException("Filters.Type expect UpdateType as parameter. Sould be like: [Filter(Filters.Type, Param: UpdateType.Message)]");
        }

        return ctx.Update.Type == type;
    }

    public static bool FilterCallbackQuery(IUpdateContext ctx)
    {
        var update = ctx.Update;

        return update.Type == UpdateType.CallbackQuery;
    }

    public static bool FilterGlobalState(IUpdateContext ctx)
    {
        var Store = ctx.Services.GetRequiredService<IKeyValueStorage>();
        var result = Store!.Contain(ctx.GetSafeUserId().GetValueOrDefault(), Consts.GLOBAL_STATE);
        if(result.IsCompleted)
        {
            return result.Result;
        }
        else
        {
            return result.AsTask().GetAwaiter().GetResult();
        }
    }

    public static bool FilterNotGlobalState(IUpdateContext ctx)
    {
        return !FilterGlobalState(ctx);
    }

    private static readonly Type _baseStateControllerType = typeof(BotControllerState<>);
    public static bool FilterCurrentGlobalState(IUpdateContext ctx)
    {
        var handler = ctx.GetCurrentHandler();

        if(handler == null)
        {
            return false;
        }

        var stateType = GetStateType(handler.TargetMethod.DeclaringType!);
        if(stateType == null)
        {
            throw new BotfException($"The filter's Filters.CurrentGlobalState method {handler.TargetMethod.Name} of class {handler.TargetMethod.DeclaringType!.Name} must be declared inside subclass of BotControllerState<>");
        }

        var userId = ctx.GetSafeUserId();
        if(!userId.HasValue)
        {
            return false;
        }

        var Store = ctx.Services.GetRequiredService<IKeyValueStorage>();
        var containsTask = Store!.Contain(userId.Value, Consts.GLOBAL_STATE);
        var contains = false;
        if(containsTask.IsCompleted)
        {
            contains = containsTask.Result;
        }
        else
        {
            contains = containsTask.AsTask().GetAwaiter().GetResult();
        }

        if(!contains)
        {
            return false;
        }

        var stateTask = Store!.Get(userId.Value, Consts.GLOBAL_STATE, null);
        object? state = null;
        if(stateTask.IsCompleted)
        {
            state = stateTask.Result;
        }
        else
        {
            state = stateTask.AsTask().GetAwaiter().GetResult();
        }

        if(state == null)
        {
            return false;
        }

        return state.GetType() == stateType;

        static Type? GetStateType(Type declatarion)
        {
            var current = declatarion;
            while(current != null)
            {
                if(current.IsGenericType && current.GetGenericTypeDefinition() == _baseStateControllerType)
                {
                    return current.GenericTypeArguments[0];
                }
                current = current.BaseType;
            }

            return null;
        }
    }

    public static bool FilterInlineQuery(IUpdateContext ctx)
    {
        var update = ctx.Update;

        if(update.Type == UpdateType.InlineQuery)
        {
            return true;
        }

        return false;
    }

    public static bool FilterCommands(IUpdateContext ctx)
    {
        var update = ctx.Update;

        if(update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text && update.Message!.Text!.StartsWith('/'))
        {
            return true;
        }

        return false;
    }

    static readonly MessageType[] _mediaMessageTypes = {
        MessageType.Audio,
        MessageType.Dice,
        MessageType.Document,
        MessageType.Photo,
        MessageType.Venue,
        MessageType.Video,
        MessageType.VideoNote,
        MessageType.Voice
    };

    public static bool FilterMedia(IUpdateContext ctx)
    {
        var update = ctx.Update;

        return update.Type == UpdateType.Message
            && _mediaMessageTypes.Contains(update.Message!.Type);
    }

    public static bool FilterDocument(IUpdateContext ctx)
    {
        var update = ctx.Update;

        return update.Type == UpdateType.Message
            && update.Message!.Type == MessageType.Document;
    }

    public static bool FilterPhoto(IUpdateContext ctx)
    {
        var update = ctx.Update;

        return update.Type == UpdateType.Message
            && update.Message!.Type == MessageType.Photo;
    }

    public static bool FilterAudio(IUpdateContext ctx)
    {
        var update = ctx.Update;

        return update.Type == UpdateType.Message
            && update.Message!.Type == MessageType.Audio;
    }

    public static bool FilterSticker(IUpdateContext ctx)
    {
        var update = ctx.Update;

        return update.Type == UpdateType.Message
            && update.Message!.Type == MessageType.Sticker;
    }

    private static readonly Dictionary<string, Regex> _regexCache = new ();
    public static bool FilterRegex(IUpdateContext ctx)
    {
        var parameter = ctx.GetFilterParameter();

        if(parameter == null)
        {
            throw new BotfException("Filters.Regex except not null string regexp in Filter parameter or Regex instance. Sould be like: [Filter(Filters.Regex, Param: @\"hello .*\\!\")]");
        }

        Regex? regex;

        if(parameter is string strParam)
        {
            if(!_regexCache.TryGetValue(strParam, out regex))
            {
                try
                {
                    regex = new Regex(strParam, RegexOptions.Compiled);
                    _regexCache[strParam] = regex;
                }
                catch(Exception e)
                {
                    throw new BotfException($"Regex compilation is failed. Check the syntax of your query 'strParam'", e);
                }
            }
        }
        else if(parameter is Regex regex1)
        {
            regex = regex1;
        }
        else
        {
            throw new BotfException("Filters.Regex except string regexp in Filter parameter or Regex instance. Sould be like: [Filter(Filters.Regex, Param: @\"hello .*\\!\")]");
        }

        var payload = ctx.GetSafeTextPayload();
        if(payload != null)
        {
            return regex.IsMatch(payload);
        }

        return false;
    }
}