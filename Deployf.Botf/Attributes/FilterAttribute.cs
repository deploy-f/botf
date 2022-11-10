using System.Reflection;

namespace Deployf.Botf;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class FilterAttribute : Attribute
{
    public readonly string Filter;
    public readonly BoolOp Operation;
    public readonly object? Param;

    public FilterAttribute(string? Or = null, string? And = null, string? OrNot = null, string? AndNot = null, string? Not = null, object? Param = null)
    {
        var arguments = new [] { Or, And, OrNot, AndNot, Not }.Where(c => c != null);
        if(arguments.Count() > 1 || !arguments.Any())
        {
            throw new BotfException("You must pass only single argument into Filter() attribute");
        }

        Filter = arguments.First()!;

        if(And != null)
        {
            Operation = BoolOp.And;
        }
        else if (Or != null)
        {
            Operation = BoolOp.Or;
        }
        else if (OrNot != null)
        {
            Operation = BoolOp.OrNot;
        }
        else if (AndNot != null)
        {
            Operation = BoolOp.AndNot;
        }
        else if (Not != null)
        {
            Operation = BoolOp.Not;
        }

        this.Param = Param;
    }

    public MethodInfo? GetMethod(Type? declaringType) => GetMethod(Filter, declaringType);

    public static MethodInfo? GetMethod(string filter, Type? declaringType)
    {
        if(filter.Contains('.'))
        {
            var typeName = filter.Substring(0, filter.LastIndexOf('.'));
            var methodName = filter.Substring(filter.LastIndexOf('.') + 1);

            var type = Type.GetType(typeName);
            if(type == null)
            {
                return null;
            }

            var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return method;
        }

        return declaringType!.GetMethod(filter, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
    }

    public enum BoolOp
    {
        Not,
        And,
        Or,
        AndNot,
        OrNot,
    }
}