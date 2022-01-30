using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;

namespace Deployf.Botf;

public class BotControllersBeforeAllMiddleware : IUpdateHandler
{
    readonly BotControllersInvoker _invoker;
    readonly BotControllerHandlers _handlers;
    readonly BotfOptions _options;

    public BotControllersBeforeAllMiddleware(BotControllersInvoker invoker, BotControllerHandlers handlers, BotfOptions options)
    {
        _invoker = invoker;
        _handlers = handlers;
        _options = options;
    }

    public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
    {
        var update = context.Update;
        var message = context.Update.Message ?? context.Update.EditedMessage;
        // avoid handling updates in group that is not addressed to bot
        if (_options.HandleOnlyMentionedInGroups
            && (update.Type == UpdateType.EditedMessage || update.Type == UpdateType.Message)
            && message!.Chat.Id != message.From!.Id // detect that we are in private chat with user
            && !((message.ReplyToMessage != null && message.ReplyToMessage.From!.Username == _options.Username)
               || (message.Text!.Contains(_options.UsernameTag!)))
        ){
            return;
        }

        if (_handlers.TryGetValue(Handle.BeforeAll, out var controller))
        {
            await _invoker.Invoke(context, cancellationToken, controller);
        }

        await next(context, cancellationToken);
    }
}