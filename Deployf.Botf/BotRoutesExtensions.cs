using System.Reflection;

namespace Deployf.Botf.Controllers
{
    public static class BotRoutesExtensions
    {
        public static string GetAuthPolicy(this MethodInfo method)
        {
            if(method.GetCustomAttribute<AllowAnonymousAttribute>() != null)
            {
                return null;
            }

            var methodAuth = method.GetCustomAttribute<AuthorizeAttribute>();
            if(methodAuth != null)
            {
                return methodAuth.Policy ?? string.Empty;
            }

            var classAuth = method.DeclaringType.GetCustomAttribute<AuthorizeAttribute>();
            if (classAuth != null)
            {
                return classAuth.Policy ?? string.Empty;
            }

            return null;
        }
    }
}