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

    public ValueTaskGeneric Decode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        if (argument is string str)
        {
            var binary = long.Parse(str);

#if NET5_0
            return new(DateTime.FromBinary(binary));
#else
            return ValueTask.FromResult<object>(DateTime.FromBinary(binary));
#endif
            
            
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
