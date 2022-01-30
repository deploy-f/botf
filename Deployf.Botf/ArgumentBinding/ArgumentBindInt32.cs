using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class ArgumentBindInt32 : IArgumentBind
{
    public bool CanDecode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(int);
    }

    public bool CanEncode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(int);
    }

    public ValueTask<object> Decode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        return new (int.Parse(argument.ToString()!));
    }

    public string Encode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        return argument.ToString()!;
    }
}
