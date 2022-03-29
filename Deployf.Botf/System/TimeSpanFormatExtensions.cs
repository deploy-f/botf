namespace Deployf.Botf;

public static class TimeSpanFormatExtensions
{
    public static TimeSpan? TryParseTimeSpan(this string format)
    {
        if(format == "-1")
        {
            return null;
        }

        if (TimeSpan.TryParse(format, out var result))
        {
            return result;
        }
        else
        {
            throw new FormatException("TimeSpan wrong format");
        }
    }
}
