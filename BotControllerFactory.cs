using System.Reflection;

namespace Deployf.TgBot.Controllers
{
    public class BotControllerFactory
    {
        public static BotControllerRoutes MakeRoutes()
        {
            var baseController = typeof(BotControllerBase);

            var keys = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(c => c.GetTypes())
                .Where(c => !c.IsAbstract && baseController.IsAssignableFrom(c))

                .Select(c => c.GetMethods()
                    .Select(c => (templ: GetTemplate(c), m: c))
                    .Where(c => c.templ != null))

                .SelectMany(c => c)

                .ToDictionary(c => c.templ, c => c.m);

            return new BotControllerRoutes(keys);
        }

        private static string GetTemplate(MethodInfo method)
        {
            var route = method.GetCustomAttribute<ActionAttribute>();
            if(route != null)
            {
                return route.Template ?? method.Name;
            }

            return null;
        }

        public static BotControllerStates MakeStates()
        {
            var baseController = typeof(BotControllerBase);

            var keys = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(c => c.GetTypes())
                .Where(c => !c.IsAbstract && baseController.IsAssignableFrom(c))

                .Select(c => c.GetMethods()
                    .Where(c => c.GetParameters().Length == 1)
                    .Where(c => c.GetCustomAttribute<StateAttribute>() != null)
                    .Select(c => (type: c.GetParameters()[0].ParameterType, m: c)))

                .SelectMany(c => c)

                .ToDictionary(c => c.type, c => c.m);

            return new BotControllerStates(keys);
        }
    }
}