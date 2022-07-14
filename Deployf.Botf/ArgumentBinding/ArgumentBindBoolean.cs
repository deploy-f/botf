using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

#if NET5_0
    using ValueTask = System.Threading.Tasks.ValueTask;
    using ValueTaskGeneric = System.Threading.Tasks.ValueTask<object>;
#else
using ValueTask = System.Threading.Tasks.Task;
using ValueTaskGeneric = System.Threading.Tasks.Task<object>;
#endif

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

    public ValueTaskGeneric Decode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
#if NET5_0
            return new(argument.ToString()! == "1");
#else
        return ValueTask.FromResult<object>(argument.ToString()! == "1");
#endif
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
