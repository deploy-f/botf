namespace Deployf.Botf;

public static class NumberExtensions
{
    public static long Base64(this string base64)
    {
        if (base64.Length % 4 != 0)
        {
            base64 += "===".Substring(0, 4 - (base64.Length % 4));
        }
        var bytes = Convert.FromBase64String(base64.Replace("-", "/"));
        return BitConverter.ToInt64(bytes);
    }

    public static string Base64(this long value)
    {
        var bytes = BitConverter.GetBytes(value);
        return Convert.ToBase64String(bytes).Replace("/", "-").Replace("=", "");
    }
}