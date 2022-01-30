using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class ArgumentBindDateTime : IArgumentBind
{
    public bool CanDecode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(DateTime);
    }

    public bool CanEncode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(DateTime);
    }

    public ValueTask<object> Decode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        if (argument is string str)
        {
            var binary = long.Parse(str);
            return new(DateTime.FromBinary(binary));
        }
        return ValueTask.FromException<object>(new NotImplementedException("not implemented convertion"));
    }

    public string Encode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        if (argument is DateTime dt)
        {
            var binary = dt.ToBinary();
            return binary.ToString();
        }
        throw new NotImplementedException("not implemented convertion");
    }
}
