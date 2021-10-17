using Deployf.BL.Objects;
using Deployf.TgBot.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


namespace Deployf.TgBot.Controllers
{
    public abstract class BotControllerBase
    {
        public UserClaims User { get; set; }
        protected long ChatId { get; private set; }
        protected IUpdateContext Context { get; private set; }
        protected CancellationToken CancelToken { get; private set; }
        protected ITelegramBotClient Client { get; set; }

        protected StringBuilder _bufferedMessage;
        protected IReplyMarkup _markup;

        protected List<List<InlineKeyboardButton>> _reply;

        public virtual void Init(IUpdateContext context, CancellationToken cancellationToken)
        {
            Context = context;
            CancelToken = cancellationToken;
            ChatId = context.GetSafeChatId().GetValueOrDefault();
            Client = Context.Bot.Client;
        }

        public async Task SendOrUpdate()
        {
            if (Context.Update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
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
            await Context.Bot.Client.SendTextMessageAsync(
                Context.GetSafeChatId(),
                text,
                ParseMode.Html,
                replyMarkup: _markup,
                cancellationToken: CancelToken);
        }

        public async Task UpdateMarkup(InlineKeyboardMarkup markup)
        {
            await Context.Bot.Client.EditMessageReplyMarkupAsync(
                Context.GetSafeChatId(),
                Context.GetSafeMessageId().Value,
                markup,
                cancellationToken: CancelToken
            );
        }

        public async Task Update(InlineKeyboardMarkup markup = null, string text = null, ParseMode mode = ParseMode.Html)
        {
            await Context.Bot.Client.EditMessageTextAsync(
                Context.GetSafeChatId(),
                Context.GetSafeMessageId().Value,
                text ?? _bufferedMessage?.ToString(),
                parseMode: mode,
                replyMarkup: markup ?? (InlineKeyboardMarkup)_markup,
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

        protected async Task AnswerCallback(string text)
        {
            await Context.Bot.Client.AnswerCallbackQueryAsync(Context.GetCallbackQuery().Id,
                text,
                cancellationToken: CancelToken);
        }

        protected void Markup(IReplyMarkup markup)
        {
            _markup = markup;
        }

        protected void State(object state)
        {
            var sm = Context.Services.GetRequiredService<IChatFSM>();
            sm.Set(ChatId, state);
        }

        protected void PushL(string line = "")
        {
            Push(line);
            Push();
        }

        protected void Push(string line = "")
        {
            if (_bufferedMessage == null)
            {
                _bufferedMessage = new StringBuilder();
            }

            _bufferedMessage.AppendLine(line);
        }

        public async Task Send()
        {
            if (_bufferedMessage != null)
            {
                await Send(_bufferedMessage.ToString());
                _bufferedMessage = null;
                _reply = null;
                _markup = null;
            }
        }

        public async Task Call<T>(Func<T, Task> method) where T : BotControllerBase
        {
            var controller = Context.Services.GetRequiredService<T>();
            controller.Init(Context, CancelToken);
            controller.User = User;
            controller._bufferedMessage = _bufferedMessage;
            controller._reply = _reply;
            controller._markup = _markup;
            await method(controller);
        }

        public async Task SendHtml()
        {
            if (_bufferedMessage != null)
            {
                await SendHtml(_bufferedMessage.ToString());
            }
        }

        public void MakeButtonRow()
        {
            if(_reply == null)
            {
                _reply = new List<List<InlineKeyboardButton>>();
            }

            _reply.Add(new List<InlineKeyboardButton>());
        }

        public void RowButton(string text, string payload)
        {
            MakeButtonRow();
            Button(text, payload);
        }

        public void RowButton(InlineKeyboardButton button)
        {
            MakeButtonRow();
            Button(button);
        }

        public void Button(string text, string payload)
        {
            Button(InlineKeyboardButton.WithCallbackData(text, payload));
        }

        public string Path(string action, params object[] args)
        {
            return FPath(GetType().Name, action, args);
        }

        public string Q(Func<Task> noArgs)
        {
            return FPath(noArgs.Method.DeclaringType.Name, noArgs.Method.Name);
        }

        public string Q<T>(Expression<Func<T, Func<Task>>> noArgs) where T : BotControllerBase
        {
            dynamic param = noArgs;
            var name = param.Body.Operand.Object.Value.Name;
            return FPath(typeof(T).Name, name);
        }

        public string Q<T>(Func<T, Task> oneArg, object arg)
        {
            return FPath(oneArg.Method.DeclaringType.Name, oneArg.Method.Name, arg);
        }

        public string Q<T, F>(Func<T, F, Task> oneArg, object arg, object arg2)
        {
            return FPath(oneArg.Method.DeclaringType.Name, oneArg.Method.Name, arg, arg2);
        }

        public string Q<T, F, C>(Func<T, F, C, Task> oneArg, object arg, object arg2, object arg3)
        {
            return FPath(oneArg.Method.DeclaringType.Name, oneArg.Method.Name, arg, arg2, arg3);
        }

        public string FPath(string controller, string action, params object[] args)
        {
            var routes = Context.Services.GetRequiredService<BotControllerRoutes>();
            var hit = routes.FindTemplate(controller, action);
            if(hit.template == null)
            {
                throw new KeyNotFoundException($"Item with controller and action ({controller}, {action}) not found");
            }
            
            if(hit.method.GetParameters().Length != args.Length)
            {
                throw new IndexOutOfRangeException($"Argument lengths not equals");
            }

            var splitter = hit.template.StartsWith("/") ? " " : "/";

            var part2 = string.Join(splitter, args.Select(c => c.ToString()));
            return $"{hit.template}{splitter}{part2}".TrimEnd('/').TrimEnd();
        }

        public void Button(InlineKeyboardButton button)
        {
            if (_reply == null)
            {
                MakeButtonRow();
            }

            _reply.Last().Add(button);
            _markup = new InlineKeyboardMarkup(_reply.Where(c => c.Count > 0));
        }

        public void Pager<T>(PageDto<T> page, Func<T, (string text, string data)> row, string format)
        {
            var i = 0;
            foreach (var item in page.Items)
            {
                var r = row(item);
                if (i != 0 && (i % 2) == 0)
                {
                    RowButton(r.text, r.data);
                }
                else
                {
                    Button((string)r.text, (string)r.data);
                }
                i++;
            }

            MakeButtonRow();

            if (page.Page > 0)
            {
                Button($"⬅️", string.Format(format, page.Page - 1));
            }

            if (page.Page < ((page.Count / (float)page.ItemsPerPage) - 1))
            {
                Button($"➡️", string.Format(format, page.Page + 1));
            }
        }
    }
}
