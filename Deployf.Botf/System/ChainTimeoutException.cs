namespace Deployf.Botf;

public class ChainTimeoutException : Exception
{
    public readonly bool Handled;

    public ChainTimeoutException(bool handled)
    {
        Handled = handled;
    }
}
