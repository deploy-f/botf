namespace Deployf.Botf;

[System.Serializable]
public class BotfException : System.Exception
{
    public BotfException() { }
    public BotfException(string message) : base(message) { }
    public BotfException(string message, System.Exception inner) : base(message, inner) { }
    protected BotfException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
