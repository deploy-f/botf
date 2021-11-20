using System.Text;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Deployf.Botf;

public class MessageBuilder
{
    public long ChatId { get; private set; }
    public StringBuilder BufferedMessage { get; private set; } = new StringBuilder();
    public IReplyMarkup? Markup { get; set; }
    public List<List<InlineKeyboardButton>>? Reply { get; set; }
    public int ReplyToMessageId { get; set; } = 0;
    public ParseMode ParseMode { get; set; } = ParseMode.Default;
    public bool IsDirty { get; set; }

    public string Message => BufferedMessage?.ToString() ?? string.Empty;

    public MessageBuilder SetChatId(long id)
    {
        ChatId = id;
        return this;
    }

    public MessageBuilder SetParseMode(ParseMode mode)
    {
        ParseMode = mode;
        return this;
    }

    public MessageBuilder SetMarkup(IReplyMarkup markup)
    {
        Markup = markup;
        IsDirty = true;
        return this;
    }

    public MessageBuilder PushL(string line = "")
    {
        Push(line + "\n");
        return this;
    }

    public MessageBuilder Push(string line = "")
    {
        if (BufferedMessage == null)
        {
            BufferedMessage = new StringBuilder();
        }

        BufferedMessage.Append(line);
        IsDirty = true;
        return this;
    }

    public MessageBuilder MakeButtonRow()
    {
        if (Reply == null)
        {
            Reply = new List<List<InlineKeyboardButton>>();
        }

        Reply.Add(new List<InlineKeyboardButton>());
        IsDirty = true;
        return this;
    }

    public MessageBuilder RowButton(string text, string payload)
    {
        MakeButtonRow();
        Button(text, payload);
        return this;
    }

    public MessageBuilder LineButton(string text, string payload)
    {
        MakeButtonRow();
        Button(text, payload);
        MakeButtonRow();
        return this;
    }

    public MessageBuilder RowButton(InlineKeyboardButton button)
    {
        MakeButtonRow();
        Button(button);
        return this;
    }

    public MessageBuilder Button(string text, string payload)
    {
        Button(InlineKeyboardButton.WithCallbackData(text, payload));
        return this;
    }

    public MessageBuilder Button(InlineKeyboardButton button)
    {
        if (Reply == null)
        {
            MakeButtonRow();
        }

        Reply!.Last().Add(button);
        Markup = new InlineKeyboardMarkup(Reply!.Where(c => c.Count > 0));
        IsDirty = true;
        return this;
    }

    public MessageBuilder Pager<T>(Paging<T> page, Func<T, (string text, string data)> row, string format, int buttonsInRow = 2)
    {
        var i = 0;
        foreach (var item in page.Items)
        {
            var r = row(item);
            if (i != 0 && (i % buttonsInRow) == 0)
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

        if (page.PageNumber > 0)
        {
            Button($"⬅️", string.Format(format, page.PageNumber - 1));
        }

        if (page.PageNumber < ((page.Count / (float)page.ItemsPerPage) - 1))
        {
            Button($"➡️", string.Format(format, page.PageNumber + 1));
        }

        return this;
    }

    public MessageBuilder ReplyTo(int messageId = default)
    {
        ReplyToMessageId = messageId;
        return this;
    }
}