using Telegram.Bot.Types.Enums;

namespace Deployf.Botf;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class ActionAttribute : Attribute
{
    public readonly string? Template;
    public readonly string? Desc;
//    public readonly UpdateType[] UpdateTypes;
//    public readonly MessageType[] MessageTypes;

    public ActionAttribute(string? template = null, string? desc = null)
    {
        Template = template;
        Desc = desc;
    }
}
