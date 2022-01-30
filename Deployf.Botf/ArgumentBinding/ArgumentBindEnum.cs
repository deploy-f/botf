using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class ArgumentBindEnum : IArgumentBind
{
    public bool CanDecode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType.IsEnum;
    }

    public bool CanEncode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType.IsEnum;
    }

    public ValueTask<object> Decode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        var str = argument.ToString();
        if (Enum.TryParse(parameter.ParameterType, str, out var result))
        {
            return new(result!);
        }
        return ValueTask.FromException<object>(new NotImplementedException("enum conversion for current data is not implemented"));
    }

    public string Encode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        return Enum.Format(parameter.ParameterType, argument, "D");
    }
}
