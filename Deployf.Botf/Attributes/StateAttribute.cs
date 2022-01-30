namespace Deployf.Botf;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public sealed class StateAttribute : Attribute
{
    public readonly string? Name;
    public readonly object? DefauleValue;

    public StateAttribute(string? name = null, object? defauleValue = null)
    {
        Name = name;
        DefauleValue = defauleValue;
    }
}