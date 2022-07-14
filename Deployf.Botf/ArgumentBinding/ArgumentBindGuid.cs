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

    public ValueTaskGeneric Decode(ParameterInfo parameter, object argument, IUpdateContext _)
    {
        if (argument is string str)
        {
#if NET5_0
            return new(new Guid(Convert.FromBase64String(str.Replace('_', '/'))));
#else
            return ValueTask.FromResult<object>(new Guid(Convert.FromBase64String(str.Replace('_', '/'))));
#endif
            
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