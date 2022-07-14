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

public class ArgumentBindSingle : IArgumentBind
{
    public bool CanDecode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(float);
    }

    public bool CanEncode(ParameterInfo parameter, object argument)
    {
        return parameter.ParameterType == typeof(float);
    }

    public ValueTaskGeneric Decode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
#if NET5_0
            return new (float.Parse(argument.ToString()!));
#else
        return ValueTask.FromResult<object>(float.Parse(argument.ToString()!));
#endif
        
    }

    public string Encode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        return argument.ToString()!;
    }
}
