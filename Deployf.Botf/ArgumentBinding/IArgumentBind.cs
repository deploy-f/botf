using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.Botf;

public interface IArgumentBind
{
    bool CanDecode(ParameterInfo parameter, object argument);
    bool CanEncode(ParameterInfo parameter, object argument);

    string Encode(ParameterInfo parameter, object argument, IUpdateContext context);
    ValueTask<object> Decode(ParameterInfo parameter, object argument, IUpdateContext context);
}
