namespace Deployf.Botf;

public class BotfOptions
{
    public string? Token { get; set; }
    public string? Username { get; set; }
    public string? WebhookUrl { get; set; }
    public bool AutoSend { get; set; }
    public bool UseWebhooks => !string.IsNullOrEmpty(WebhookUrl);
    public string WebhookPath
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
}