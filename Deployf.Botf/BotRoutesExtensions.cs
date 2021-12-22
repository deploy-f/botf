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

        var classAuth = method.DeclaringType!.GetCustomAttribute<AuthorizeAttribute>();
        if (classAuth != null)
        {
            return classAuth.Policy ?? string.Empty;
        }

        return null;
    }

    public static string? GetActionDescription(this MethodInfo method)
    {
        var action = method.GetCustomAttributes<ActionAttribute>().FirstOrDefault(c => c.Desc != null);
        if (action == null)
        {
            return null;
        }

        return action.Desc;
    }

    public static string Truncate(this string str, int length)
    {
        if(str.Length <= length)
        {
            return str;
        }

        return str.Substring(0, length);
    }
    public static string TruncateEnd(this string str, int length)
    {
        if (str.Length <= length)
        {
            return str;
        }
        return str.Substring(str.Length - length - 1);
    }

    public static long GetDeterministicHashCode(this string str)
    {
        unchecked
        {
            long hash1 = (5381 << 16) + 5381;
            long hash2 = hash1;

            for (int i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
}

public static class NumberExtensions
{
    public static long Base64(this string base64)
    {
        if (base64.Length % 4 != 0)
        {
            base64 += "===".Substring(0, 4 - (base64.Length % 4));
        }
        var bytes = Convert.FromBase64String(base64);
        return BitConverter.ToInt64(bytes);
    }

    public static string Base64(this long value)
    {
        var bytes = BitConverter.GetBytes(value);
        return Convert.ToBase64String(bytes).Replace("=", "");
    }
}