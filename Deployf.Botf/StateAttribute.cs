namespace Deployf.Botf;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class StateAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ActionAttribute : Attribute
{
    public readonly string? Template;

    public ActionAttribute(string? template = null)
    {
        Template = template;
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