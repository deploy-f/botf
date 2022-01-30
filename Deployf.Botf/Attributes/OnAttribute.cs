namespace Deployf.Botf;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class OnAttribute : Attribute
{
    public readonly Handle Handler;

    public OnAttribute(Handle type)
    {
        Handler = type;
    }
}
