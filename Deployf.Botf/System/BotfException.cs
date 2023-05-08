using System.Runtime.Serialization;

namespace Deployf.Botf;

[Serializable]
public class BotfException : Exception
{
    public BotfException() { }
    public BotfException(string message) : base(message) { }
    public BotfException(string message, Exception inner) : base(message, inner) { }
    protected BotfException(
        SerializationInfo info,
        StreamingContext context) : base(info, context) { }
}
