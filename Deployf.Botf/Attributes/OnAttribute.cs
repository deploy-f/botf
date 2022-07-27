using Telegram.Bot.Types.Enums;

namespace Deployf.Botf;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class OnAttribute : Attribute
{
    public readonly Handle Handler;
//    public readonly UpdateType[] UpdateTypes;
//    public readonly MessageType[] MessageTypes;

    public OnAttribute(Handle type)
    {
        Handler = type;
    }

/*
    public OnAttribute(Handle type, params UpdateType[] updateTypes)
    {
        Handler = type;
        UpdateTypes = updateTypes;
    }

    public OnAttribute(Handle type, params MessageType[] messageTypes)
    {
        Handler = type;
        MessageTypes = messageTypes;
    }

    public OnAttribute(Handle type, UpdateType[] updateTypes, MessageType[] messageTypes)
    {
        Handler = type;
        MessageTypes = messageTypes;
        UpdateTypes = updateTypes;
    }
*/
}