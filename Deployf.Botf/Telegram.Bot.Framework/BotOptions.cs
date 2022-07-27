using Telegram.Bot.Framework.Abstractions;

namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Configurations for the bot
    /// </summary>
    /// <typeparam name="TBot">Type of Bot</typeparam>
    public class BotOptions<TBot> : IBotOptions
        where TBot : IBot
    {
        public string Username { get; set; }

        /// <summary>
        /// Optional if client not needed. Telegram API token
        /// </summary>
        public string ApiToken { get; set; }

        public string WebhookPath { get; set; }

        public BotOptions(string username, string apiToken, string webhookPath)
        {
            Username = username;
            ApiToken = apiToken;
            WebhookPath = webhookPath;
        }
    }
}