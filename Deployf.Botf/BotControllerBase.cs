using System.Diagnostics;
using System.Linq.Expressions;
using Telegram.Bot;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Deployf.Botf;

public abstract class BotControllerBase
{
    public UserClaims User { get; set; } = new UserClaims();
    protected long ChatId { get; private set; }
    protected long FromId { get; private set; }
    protected IUpdateContext Context { get; private set; } = null!;
    protected CancellationToken CancelToken { get; private set; }
    protected ITelegramBotClient Client { get; set; } = null!;
    protected MessageBuilder Message { get; set; } = new MessageBuilder();
    protected IKeyValueStorage? Store { get; set; }
    protected IViewProvider? ViewProvider { get; set; }
    protected IPathQuery? Query { get; set; }

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
        ViewProvider = Context.Services.GetRequiredService<IViewProvider>();
        Query = Context.Services.GetRequiredService<IPathQuery>();
        Message = new MessageBuilder();
    }

    protected ValueTask State(object state)
    {
        return Store!.Set(FromId, BotControllersFSMMiddleware.STATE_KEY, state);
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

    public async Task Call<T>(Func<T, Task> method) where T : BotControllerBase
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

    public virtual void View(string viewName, object model)
    {
        var action = new StackFrame(1).GetMethod();
        ViewProvider!.ExecuteView(new (viewName, Message, model, this, action));
    }

    #region sending
    public async Task SendOrUpdate()
    {
        if (Context!.Update.Type == UpdateType.CallbackQuery)
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
        await Context!.Bot.Client.SendTextMessageAsync(
            Context!.GetSafeChatId(),
            text,
            ParseMode.Html,
            replyMarkup: Message.Markup,
            cancellationToken: CancelToken,
            replyToMessageId: Message.ReplyToMessageId);
    }

    public async Task UpdateMarkup(InlineKeyboardMarkup markup)
    {
        await Context!.Bot.Client.EditMessageReplyMarkupAsync(
            Context!.GetSafeChatId(),
            Context!.GetSafeMessageId().GetValueOrDefault(),
            markup,
            cancellationToken: CancelToken
        );
    }

    public async Task Update(InlineKeyboardMarkup? markup = null, string? text = null, ParseMode mode = ParseMode.Html)
    {
        IsDirty = false;
        await Context!.Bot.Client.EditMessageTextAsync(
            Context!.GetSafeChatId(),
            Context!.GetSafeMessageId().GetValueOrDefault(),
            text ?? Message.Message,
            parseMode: mode,
            replyMarkup: markup ?? Message.Markup as InlineKeyboardMarkup,
            cancellationToken: CancelToken
        );
    }

    protected async Task SendHtml(string text)
    {
        await Send(text, ParseMode.Html);
    }

    protected async Task Send(string text)
    {
        await Send(text, ParseMode.Default);
    }

    protected async Task AnswerCallback(string? text = null)
    {
        await Context!.Bot.Client.AnswerCallbackQueryAsync(Context!.GetCallbackQuery().Id,
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

    public async Task SendHtml()
    {
        Message.SetParseMode(ParseMode.Html);
        var text = Message.Message;
        if (text != null)
        {
            await SendHtml(text);
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

    public string Q<T>(Expression<Func<T, Func<Task>>> noArgs) where T : BotControllerBase
    {
        dynamic param = noArgs;
        var name = param.Body.Operand.Object.Value.Name;
        return FPath(typeof(T).Name, name);
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
        return Query!.GetPath(controller, action, args);
    }
    #endregion
}