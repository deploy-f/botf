using System;

namespace Deployf.TgBot.Controllers
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class StateAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ActionAttribute : Attribute
    {
        public readonly string Template;

        public ActionAttribute(string template = null)
        {
            Template = template;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class AuthorizeAttribute : Attribute
    {
        public readonly string Policy;

        public AuthorizeAttribute(string policy = null)
        {
            Policy = policy;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class AllowAnonymousAttribute : Attribute
    {
    }
}