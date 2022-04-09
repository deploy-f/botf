using System.Linq.Expressions;
using Telegram.Bot;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Deployf.Botf;

public abstract class BotController
{
    private const string LAST_MESSAGE_ID_KEY = "$last_message_id";

    public UserClaims User { get; set; } = new UserClaims();
    protected long ChatId { get; private set; }
    protected long FromId { get; private set; }
    protected IUpdateContext Context { get; private set; } = null!;
    protected CancellationToken CancelToken { get; private set; }
    protected ITelegramBotClient Client { get; set; } = null!;
    protected MessageBuilder Message { get; set; } = new MessageBuilder();
    protected IKeyValueStorage? Store { get; set; }

    protected bool IsDirty
    {
        get => Message.IsDirty;
        set => Message.IsDirty = value;
    }

    public virtual void Init(IUpdateContext context, CancellationToken cancellationToken)
    {
        Context = context;
        CancelToken = cancellationToken;
        ChatId = Context.GetSafeChatId().GetValueOrDefault();
        FromId = Context.GetSafeUserId().GetValueOrDefault();
        Client = Context.Bot.Client;
        Store = Context.Services.GetService<IKeyValueStorage>(); // todo: move outside
        Message = new MessageBuilder();
    }

    #region state management
    protected ValueTask State(object state)
    {
        return Store!.Set(FromId, BotControllersFSMMiddleware.STATE_KEY, state);
    }

    protected ValueTask ClearState()
    {
        return Store!.Remove(FromId, BotControllersFSMMiddleware.STATE_KEY);
    }

    protected ValueTask AState<T>(T state, string? name = null) where T : notnull
    {
        if (name == null)
        {
            return Store!.Set(FromId, typeof(T).Name, state);
        }
        return Store!.Set(FromId, name, state);
    }

    protected ValueTask<T?> GetAState<T>(string? name = null, T? def = default)
    {
        if (name == null)
        {
            return Store!.Get(FromId, typeof(T).Name, def);
        }
        return Store!.Get(FromId, name, def);
    }

    protected async ValueTask GlobalState(object? state)
    {
        if(state == null)
        {
            if(await Store!.Contain(FromId, Consts.GLOBAL_STATE))
            {
                var oldState = await Store!.Get(FromId, Consts.GLOBAL_STATE, null);
                if (oldState != null)
                {
                    await Call(true, oldState);
                }
            }
            await Store!.Remove(FromId, Consts.GLOBAL_STATE);
            await CallClear();
        }
        else
        {
            if (await Store!.Contain(FromId, Consts.GLOBAL_STATE))
            {
                var oldState = await Store!.Get(FromId, Consts.GLOBAL_STATE, null);
                if (oldState != null)
                {
                    await Call(true, oldState);
                }
            }
            await Store!.Set(FromId, Consts.GLOBAL_STATE, state);
            await Call(false, state);
        }

        async ValueTask Call(bool leave, object oldState)
        {
            var routes = Context!.Services.GetRequiredService<BotControllerRoutes>();
            var controllerType = routes.GetStateType(oldState.GetType());
            if (controllerType != null)
            {
                var controller = (BotControllerState)Context!.Services.GetRequiredService(controllerType);
                controller.Init(Context, CancelToken);
                controller.User = User;
                await controller.OnBeforeCall();
                if (leave)
                {
                    await controller.OnLeave();
                }
                else
                {
                    await controller.OnEnter();
                }
                await controller.OnAfterCall();
            }
        }

        async ValueTask CallClear()
        {
            var handlers = Context!.Services.GetRequiredService<BotControllerHandlers>();
            if(handlers.TryGetValue(Handle.ClearState, out var method))
            {
                var invoker = Context!.Services.GetRequiredService<BotControllersInvoker>();
                await invoker.Invoke(Context, CancelToken, method);
            }
        }
    }
    #endregion

    #region misc
    public async Task Call<T>(Func<T, Task> method) where T : BotController
    {
        var controller = Context!.Services.GetRequiredService<T>();
        controller.Init(Context, CancelToken);
        controller.User = User;
        controller.Message = Message;
        controller.Store = Store;
        await controller.OnBeforeCall();
        await method(controller);
        await controller.OnAfterCall();
    }

    public async Task Call<T>(Action<T> method) where T : BotController
    {
        var controller = Context!.Services.GetRequiredService<T>();
        controller.Init(Context, CancelToken);
        controller.User = User;
        controller.Message = Message;
        controller.Store = Store;
        await controller.OnBeforeCall();
        method(controller);
        await controller.OnAfterCall();
    }

    public virtual Task OnBeforeCall()
    {
        return Task.CompletedTask;
    }

    public virtual async Task OnAfterCall()
    {
        if (!(Context!.Bot is BotfBot bot))
        {
            return;
        }

        if (bot.Options.AutoSend && IsDirty)
        {
            await SendOrUpdate();
        }
    }
    #endregion

    #region sending
    public async Task SendOrUpdate()
    {
        if (Context!.Update.Type == UpdateType.CallbackQuery && Message.Markup is not ReplyKeyboardMarkup)
        {
            await Update();
        }
        else
        {
            await Send();
        }
    }

    protected async Task Send(string text, ParseMode mode)
    {
        IsDirty = false;
        Message message;
        if(Message.PhotoUrl == null)
        {
            message = await Client.SendTextMessageAsync(
                Context!.GetSafeChatId()!,
                text,
                ParseMode.Html,
                replyMarkup: Message.Markup,
                cancellationToken: CancelToken,
                replyToMessageId: Message.ReplyToMessageId);
        }
        else
        {
            message = await Client.SendPhotoAsync(
                Context!.GetSafeChatId()!,
                Message.PhotoUrl,
                text,
                ParseMode.Html,
                replyMarkup: Message.Markup,
                cancellationToken: CancelToken,
                replyToMessageId: Message.ReplyToMessageId);
        }
        await TryCleanLastMessageReplyKeyboard();
        await TrySaveLastMessageId(Message.Markup as InlineKeyboardMarkup, message);
        ClearMessage();
    }

    public async Task UpdateMarkup(InlineKeyboardMarkup markup)
    {
        await Client.EditMessageReplyMarkupAsync(
            Context!.GetSafeChatId()!,
            Context!.GetSafeMessageId().GetValueOrDefault(),
            markup,
            cancellationToken: CancelToken
        );
    }

    public async Task Update(InlineKeyboardMarkup? markup = null, string? text = null, ParseMode mode = ParseMode.Html)
    {
        var markupValue = markup ?? Message.Markup as InlineKeyboardMarkup;
        IsDirty = false;
        var message = await Client.EditMessageTextAsync(
            Context!.GetSafeChatId()!,
            Context!.GetSafeMessageId().GetValueOrDefault(),
            text ?? Message.Message,
            parseMode: mode,
            replyMarkup: markupValue,
            cancellationToken: CancelToken
        );
        await TrySaveLastMessageId(markupValue, message);
        ClearMessage();
    }

    protected async Task Send(string text)
    {
        await Send(text, ParseMode.Html);
    }

    protected async Task AnswerCallback(string? text = null)
    {
        await Client.AnswerCallbackQueryAsync(Context!.GetCallbackQuery().Id,
            text,
            cancellationToken: CancelToken);
    }

    public async Task Send()
    {
        var text = Message.Message;
        if (text != null)
        {
            await Send(text);
        }
    }

    private async ValueTask TrySaveLastMessageId(InlineKeyboardMarkup? markupValue, Telegram.Bot.Types.Message message)
    {
        try
        {
            if (Context!.Bot is BotfBot bot && bot.Options.AutoCleanReplyKeyboard)
            {
                if (markupValue != null)
                {
                    await Store!.Set(FromId, LAST_MESSAGE_ID_KEY, $"{message.Chat.Id};{message.MessageId}");
                }
                else
                {
                    await Store!.Remove(FromId, LAST_MESSAGE_ID_KEY);
                }
            }
        }
        catch (Exception e)
        {
            var logger = Context.Services.GetService<ILogger<BotController>>();
            logger?.LogError(e, "Error while trying to set last message reply keyboard for clean it in future");
        }
    }

    private async ValueTask TryCleanLastMessageReplyKeyboard()
    {
        try
        {
            if (Context!.Bot is BotfBot bot && bot.Options.AutoCleanReplyKeyboard)
            {
                if (await Store!.Contain(FromId, LAST_MESSAGE_ID_KEY))
                {
                    var data = await Store.Get<string>(FromId, LAST_MESSAGE_ID_KEY, null);
                    if (data != null)
                    {
                        var items = data.Split(';');
                        var chatId = long.Parse(items[0]);
                        var messageId = int.Parse(items[1]);
                        await Client.EditMessageReplyMarkupAsync(chatId, messageId, null);
                    }
                }
            }
        }
        catch(Exception e)
        {
            var logger = Context.Services.GetService<ILogger<BotController>>();
            logger?.LogError(e, "Error while trying to clean last message reply keyboard");
        }
    }
    #endregion

    #region formatting
    protected void Markup(IReplyMarkup markup)
    {
        Message.SetMarkup(markup);
    }

    protected void PushL(string line = "")
    {
        Message.PushL(line);
    }

    protected void PushLL(string line = "")
    {
        Message.PushL(line);
        Message.PushL();
    }

    protected void Push(string line = "")
    {
        Message.Push(line);
    }

    public void MakeButtonRow()
    {
        Message.MakeButtonRow();
    }

    public void RowButton(string text, string payload)
    {
        Message.RowButton(text, payload);
    }

    public void RowButton(InlineKeyboardButton button)
    {
        Message.RowButton(button);
    }

    public void Button(string text, string payload)
    {
        Message.Button(text, payload);
    }

    public void Button(InlineKeyboardButton button)
    {
        Message.Button(button);
    }

    public void MakeKButtonRow()
    {
        Message.MakeKButtonRow();
    }

    public void RowKButton(string text)
    {
        Message.RowKButton(text);
    }

    public void RowKButton(KeyboardButton button)
    {
        Message.RowKButton(button);
    }

    public void KButton(string text)
    {
        Message.KButton(text);
    }

    public void KButton(KeyboardButton button)
    {
        Message.KButton(button);
    }

    public void Pager<T>(Paging<T> page, Func<T, (string text, string data)> row, string format, int buttonsInRow = 2)
    {
        Message.Pager(page, row, format, buttonsInRow);
    }

    public void Reply(int? messageId = default)
    {
        if(messageId == null)
        {
            Message.ReplyTo(Context!.GetSafeMessageId() ?? 0);
        }
        else
        {
            Message.ReplyTo(messageId.GetValueOrDefault());
        }
    }

    public void Photo(string url)
    {
        Message.SetPhotoUrl(url);
    }

    public void ClearMessage()
    {
        Message = new MessageBuilder();
    }
    #endregion

    #region querying
    public string Path(string action, params object[] args)
    {
        return FPath(GetType().Name, action, args);
    }

    public string Q(Delegate method, params object[] arguments)
    {
        return FPath(method.Method.DeclaringType!.Name, method.Method.Name, arguments);
    }

    public string Q(Func<Task> noArgs)
    {
        return FPath(noArgs.Method.DeclaringType!.Name, noArgs.Method.Name);
    }

    public string Q<T>(Expression<Func<T, Action>> noArgs) where T : BotController
    {
        dynamic param = noArgs;
        var name = param.Body.Operand.Object.Value.Name;
        return FPath(typeof(T).Name, name);
    }

    public string Q<T>(Expression<Func<T, Func<Task>>> noArgs) where T : BotController
    {
        dynamic param = noArgs;
        var name = param.Body.Operand.Object.Value.Name;
        return FPath(typeof(T).Name, name);
    }

    public string Q<T>(Expression<Func<T, Func<ValueTask>>> noArgs) where T : BotController
    {
        dynamic param = noArgs;
        var name = param.Body.Operand.Object.Value.Name;
        return FPath(typeof(T).Name, name);
    }

    public string Q<T, A1>(Expression<Func<T, Action<A1>>> oneArg, object arg1) where T : BotController
    {
        dynamic param = oneArg;
        var name = param.Body.Operand.Object.Value.Name;
        return FPath(typeof(T).Name, name, arg1);
    }

    public string Q<T, A1>(Expression<Func<T, Func<A1, Task>>> oneArg, object arg1) where T : BotController
    {
        dynamic param = oneArg;
        var name = param.Body.Operand.Object.Value.Name;
        return FPath(typeof(T).Name, name, arg1);
    }

    public string Q<T, A1>(Expression<Func<T, Func<A1, ValueTask>>> oneArg, object arg1) where T : BotController
    {
        dynamic param = oneArg;
        var name = param.Body.Operand.Object.Value.Name;
        return FPath(typeof(T).Name, name, arg1);
    }

    public string Q<T>(Func<T, Task> oneArg, object arg)
    {
        return FPath(oneArg.Method.DeclaringType!.Name, oneArg.Method.Name, arg);
    }

    public string Q<T, F>(Func<T, F, Task> oneArg, object arg, object arg2)
    {
        return FPath(oneArg.Method.DeclaringType!.Name, oneArg.Method.Name, arg, arg2);
    }

    public string Q<T, F, C>(Func<T, F, C, Task> oneArg, object arg, object arg2, object arg3)
    {
        return FPath(oneArg.Method.DeclaringType!.Name, oneArg.Method.Name, arg, arg2, arg3);
    }

    public string FPath(string controller, string action, params object[] args)
    {
        var routes = Context!.Services.GetRequiredService<BotControllerRoutes>();
        var binder = Context!.Services.GetRequiredService<ArgumentBinder>();
        var hit = routes.FindTemplate(controller, action, args);
        if (hit.template == null)
        {
            throw new KeyNotFoundException($"Item with controller and action ({controller}, {action}) not found");
        }

        if (hit!.method!.GetParameters().Length != args.Length)
        {
            throw new IndexOutOfRangeException($"Argument lengths not equals");
        }

        var splitter = hit.template.StartsWith("/") ? " " : "/";

        var parts = binder.Convert(hit!.method!, args, Context);
        var part2 = string.Join(splitter, parts);
        return $"{hit.template}{splitter}{part2}".TrimEnd('/').TrimEnd();
    }
    #endregion

    #region chaining

    public async Task<IUpdateContext> AwaitNextUpdate(Action onCanceled = null)
    {
        var options = ((BotfBot)Context.Bot).Options;
        var store = Context.Services.GetRequiredService<ChainStorage>();
        var tcs = new TaskCompletionSource<IUpdateContext>();
        store.Set(ChatId, new (tcs));
        if(options.ChainTimeout.HasValue)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(options.ChainTimeout.Value);
                if(tcs != null)
                {
                    store.Clear(ChatId);
                    tcs.SetCanceled();
                }
            });
        }
        
        try
        {
            var context = await tcs.Task;
            tcs = null;
            return context;
        }
        catch(TaskCanceledException)
        {
            onCanceled?.Invoke();
            throw new ChainTimeoutException(onCanceled != null);
        }
    }

    public async Task<string> AwaitText(Action onCanceled = null)
    {
        while (true)
        {
            var update = await AwaitNextUpdate(onCanceled);
            if (update.Update.Type != UpdateType.Message)
            {
                continue;
            }

            return update.GetSafeTextPayload()!;
        }
    }

    public async Task<string> AwaitQuery(Action onCanceled = null)
    {
        while (true)
        {
            var update = await AwaitNextUpdate(onCanceled);
            if (update.Update.Type != UpdateType.CallbackQuery)
            {
                continue;
            }

            return update.GetSafeTextPayload()!;
        }
    }

    #endregion
}