namespace Deployf.Botf;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class OnAttribute : Attribute
{
    public readonly Handle Handler;
    public readonly string? Filter;
    public readonly int Order;

    public OnAttribute(Handle type)
    {
        Handler = type;
        Filter = null;
        Order = 0;
    }

    [Obsolete("Use Filter() attribute instead passing filter through On")]
    public OnAttribute(Handle type, string? filter)
    {
        Handler = type;
        Filter = filter;
        Order = 0;
    }

    [Obsolete("Use Filter() attribute instead passing filter through On")]
    public OnAttribute(Handle type, string? filter, int order)
    {
        Handler = type;
        Filter = filter;
        Order = order;
    }


    public OnAttribute(Handle type, int order)
    {
        Handler = type;
        Filter = null;
        Order = order;
    }
}