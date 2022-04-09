namespace Deployf.Botf;

public static class StringExtensions
{
    public static bool IsUrl(this string data)
    {
        return data.StartsWith("https://") || data.StartsWith("http://");
    }
}