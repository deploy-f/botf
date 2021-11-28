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

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ActionAttribute : Attribute
{
    public readonly string? Template;
    public readonly string? Desc;

    public ActionAttribute(string? template = null, string? desc = null)
    {
        Template = template;
        Desc = desc;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class AuthorizeAttribute : Attribute
{
    public readonly string? Policy;

    public AuthorizeAttribute(string? policy = null)
    {
        Policy = policy;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class AllowAnonymousAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class OnAttribute : Attribute
{
    public readonly Handle Handler;

    public OnAttribute(Handle type)
    {
        Handler = type;
    }
}

public enum Handle
{
    /// <summary>
    /// Means unknown command or message type from user (telegram)
    /// </summary>
    Unknown,

    /// <summary>
    /// User isn't authorized
    /// </summary>
    Unauthorized,

    /// <summary>
    /// Handle exception
    /// </summary>
    Exception,

    /// <summary>
    /// Execute action before message go to routing and whole the botf
    /// </summary>
    BeforeAll
}