namespace Deployf.Botf;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class AuthorizeAttribute : Attribute
{
    public readonly string? Policy;

    public AuthorizeAttribute(string? policy = null)
    {
        Policy = policy;
    }
}
