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
    protected IUpdateContext? Context { get; private set; }
    protected CancellationToken CancelToken { get; private set; }
    protected ITelegramBotClient? Client { get; set; }
    protected MessageBuilder Message { get; set; } = new MessageBuilder();
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
        Message = new MessageBuilder();
    }

    protected void State(object state)
    {
        var sm = Context!.Services.GetRequiredService<IChatFSM>();
        sm.Set(ChatId, state);
    }

    public async Task Call<T>(Func<T, Task> method) where T : BotControllerBase
    {
        var controller = Context!.Services.GetRequiredService<T>();
        controller.Init(Context, CancelToken);
        controller.User = User;
        controller.Message = Message;
        await controller.OnBeforeCall();
        await method(controller);
        await controller.OnAfterCall();
    }

    public virtual async Task OnBeforeCall()
    {
    }

    public virtual async Task OnAfterCall()
    {
        if (!(Context.Bot is BotfBot bot))
        {
            return;
        }

        if (bot.Options.AutoSend && IsDirty)
        {
            await SendOrUpdate();
        }
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
        var routes = Context!.Services.GetRequiredService<BotControllerRoutes>();
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

        var part2 = string.Join(splitter, args.Select(c => c.ToString()));
        return $"{hit.template}{splitter}{part2}".TrimEnd('/').TrimEnd();
    }
    #endregion
}