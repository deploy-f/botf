using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class ArgumentBindBoolean : IArgumentBind
{
    public bool CanDecode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(bool);
    }

    public bool CanEncode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(bool);
    }

    public ValueTask<object> Decode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        return new(argument.ToString()! == "1");
    }

    public string Encode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        if(argument is bool boolValue)
        {
            return boolValue ? "1" : "0";
        }

        return argument.ToString()!;
    }
}
