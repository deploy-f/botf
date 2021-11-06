using System.Reflection;

namespace Deployf.Botf;

public static class BotRoutesExtensions
{
    public static string? GetAuthPolicy(this MethodInfo method)
    {
        if (method.GetCustomAttribute<AllowAnonymousAttribute>() != null)
        {
            return null;
        }

        var methodAuth = method.GetCustomAttribute<AuthorizeAttribute>();
        if (methodAuth != null)
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

public static class NumberExtensions
{
    public static long Base64(this string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        return BitConverter.ToInt64(bytes);
    }

    public static string Base64(this long value)
    {
        var bytes = BitConverter.GetBytes(value);
        return Convert.ToBase64String(bytes);
    }
}