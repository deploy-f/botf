using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;

namespace Deployf.Botf;

public static class Filters
{
    private const string _BASE = $"{nameof(Deployf)}.{nameof(Botf)}.{nameof(FiltersImpl)}";
    public const string Text = $"{_BASE}.{nameof(FiltersImpl.FilterNewTextMessage)}";
    public const string Messages = $"{_BASE}.{nameof(FiltersImpl.FilterAllMessages)}";
    public const string CallbackQuery = $"{_BASE}.{nameof(FiltersImpl.FilterCallbackQuery)}";
    public const string InlineQuery = $"{_BASE}.{nameof(FiltersImpl.FilterInlineQuery)}";
    public const string Command = $"{_BASE}.{nameof(FiltersImpl.FilterCommands)}";
    public const string Media = $"{_BASE}.{nameof(FiltersImpl.FilterMedia)}";
    public const string Document = $"{_BASE}.{nameof(FiltersImpl.FilterDocument)}";
    public const string Photo = $"{_BASE}.{nameof(FiltersImpl.FilterPhoto)}";
    public const string Audio = $"{_BASE}.{nameof(FiltersImpl.FilterAudio)}";
    public const string Sticker = $"{_BASE}.{nameof(FiltersImpl.FilterSticker)}";
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

        if(update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
        {
            return true;
        }

        return false;
    }

    public static bool FilterCallbackQuery(IUpdateContext ctx)
    {
        var update = ctx.Update;

        if(update.Type == UpdateType.CallbackQuery)
        {
            return true;
        }

        return false;
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

}