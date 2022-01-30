namespace Deployf.Botf;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
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
