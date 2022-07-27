namespace Telegram.Bot.Framework.Abstractions
{
    public delegate Task UpdateDelegate(IUpdateContext context, CancellationToken cancellationToken = default);
}