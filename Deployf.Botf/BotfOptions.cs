namespace Deployf.Botf;

public class BotfOptions
{
    private string? _username;

    public string? Token { get; set; }
    public string? Username
    {
        get => _username;
        set
        {
            _username = value;
            UsernameTag = "@" + value;
        }
    }

    public string? WebhookUrl { get; set; }
    public bool AutoSend { get; set; }
    public bool HandleOnlyMentionedInGroups { get; set; }

    public bool UseWebhooks => !string.IsNullOrEmpty(WebhookUrl);
    public string? WebhookPath
    {
        get
        {
            if (!string.IsNullOrEmpty(WebhookUrl))
            {
                return new Uri(WebhookUrl).PathAndQuery;
            }

            return null;
        }
    }

    public string? UsernameTag { get; private set; }
}