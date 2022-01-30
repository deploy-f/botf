using System;

namespace Deployf.Botf.Exceptions;

public class ArgumentBinderException : Exception
{
    public ArgumentBinderException(string message = "Failed to convert the passed argument") : base(message)
    {
    }
}
