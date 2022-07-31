namespace Deployf.Botf;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class OnAttribute : Attribute
{
    public readonly Handle Handler;
    public readonly string? Filter;
    public readonly int Order;

    public OnAttribute(Handle type, string? filter = null, int order = 0)
    {
        Handler = type;
        Filter = filter;
        Order = order;
    }
}