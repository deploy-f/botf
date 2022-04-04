using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public class ArgumentBindGuid : IArgumentBind
{
    public bool CanDecode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(Guid);
    }

    public bool CanEncode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(Guid);
    }

    public ValueTask<object> Decode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        if (argument is string str)
        {
            return new(new Guid(Convert.FromBase64String(str.Replace('_', '/'))));
        }
        return ValueTask.FromException<object>(new NotImplementedException("not implemented convertion"));
    }

    public string Encode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        if (argument is Guid guid)
        {
            return Convert.ToBase64String(guid.ToByteArray()).Replace('/', '_');
        }
        throw new NotImplementedException("not implemented convertion");
    }
}