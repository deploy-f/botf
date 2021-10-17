using Microsoft.Extensions.Logging;
using System.Reflection;
using Telegram.Bot.Framework.Abstractions;

namespace Deployf.TgBot.Controllers
{
    public class BotControllersInvokeMiddleware : IUpdateHandler
    {
        readonly ILogger<BotControllersInvokeMiddleware> _log;
        readonly BotControllerRoutes _map;

        public BotControllersInvokeMiddleware(BotControllerRoutes map, ILogger<BotControllersInvokeMiddleware> log)
        {
            _map = map;
            _log = log;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            if (context.Items.TryGetValue("controller", out var value) && value is BotControllerBase controller)
            {
                var method = (MethodInfo)context.Items["action"];
                var args = (object[])context.Items["args"];

                var param = method.GetParameters();
                if (args.Length != param.Length)
                {
                    throw new IndexOutOfRangeException();
                }

                var typedParams = param.Select((p, i) => (object)(p.ParameterType.Name switch
                {
                    nameof(Int32) => int.Parse(args[i].ToString()),
                    nameof(Single) => float.Parse(args[i].ToString()),
                    _ => MapDefault(p.ParameterType, args[i]),
                })).ToArray();

                _log.LogDebug("Begin execute action {Controller}.{Method}. Arguments: {@Args}",
                    method.DeclaringType.Name,
                    method.Name,
                    typedParams);

                var result = method.Invoke(controller, typedParams);
                if (result is Task task)
                {
                    await task;
                }
            }
            else
            {
                await next(context, cancellationToken);
            }
        }

        static object MapDefault(Type type, object input)
        {
            if (input.GetType() == type)
            {
                return input;
            }

            throw new NotImplementedException();
        }
    }
}