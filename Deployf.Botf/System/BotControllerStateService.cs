using System.Reflection;

namespace Deployf.Botf;

public struct BotControllerStateService
{
    public static Dictionary<Type, Func<BotController, Task>?> _savers = new ();
    public static Dictionary<Type, Func<BotController, Task>?> _loaders = new ();

    public async Task Load(BotController controller)
    {
        var controllerType = controller.GetType();
        if (_loaders.TryGetValue(controllerType, out var loader) && loader != null)
        {
            await loader(controller);
            return;
        }

        List<FieldInfo> fields;
        List<PropertyInfo> props;
        ExtractMembers(controllerType, out fields, out props);

        if (fields.Count > 0 || props.Count > 0)
        {
            loader = async (BotController _controller) =>
            {
                var storage = _controller.Store!;

                foreach (var field in fields)
                {
                    var value = await storage.Get(_controller.FromId, GetKey(field, controllerType), null);
                    field.SetValue(_controller, value);
                }

                foreach (var prop in props)
                {
                    var value = await storage.Get(_controller.FromId, GetKey(prop, controllerType), null);
                    prop.SetValue(_controller, value);
                }
            };

            _loaders[controllerType] = loader;

            await loader(controller);
        }
        else
        {
            _loaders[controllerType] = null;
        }
    }

    public async Task Save(BotController controller)
    {
        var controllerType = controller.GetType();
        if (_savers.TryGetValue(controllerType, out var saver) && saver != null)
        {
            await saver(controller);
            return;
        }

        List<FieldInfo> fields;
        List<PropertyInfo> props;
        ExtractMembers(controllerType, out fields, out props);

        if (fields.Count > 0 || props.Count > 0)
        {
            saver = async (BotController _controller) =>
            {
                var storage = _controller.Store!;

                foreach (var field in fields)
                {
                    var value = field.GetValue(_controller);
                    var key = GetKey(field, controllerType);
                    if(value != null)
                    {
                        await storage.Set(_controller.FromId, key, value);
                    }
                    else
                    {
                        await storage.Remove(_controller.FromId, key);
                    }
                }

                foreach (var prop in props)
                {
                    var value = prop.GetValue(_controller);
                    var key = GetKey(prop, controllerType);
                    if(value != null)
                    {
                        await storage.Set(_controller.FromId, key, value);
                    }
                    else
                    {
                        await storage.Remove(_controller.FromId, key);
                    }
                }
            };

            _savers[controllerType] = saver;

            await saver(controller);
        }
        else
        {
            _savers[controllerType] = null;
        }
    }

    static void ExtractMembers(Type controllerType, out List<FieldInfo> fields, out List<PropertyInfo> props)
    {
        const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        fields = controllerType
            .GetFields(bindingFlags)
            .Where(c => c.GetCustomAttribute<StateAttribute>() != null)
            .ToList();
        props = controllerType
            .GetProperties(bindingFlags)
            .Where(c => c.GetCustomAttribute<StateAttribute>() != null)
            .ToList();
    }

    static string GetKey(MemberInfo member, Type controllerType)
    {
        return $"$ctrl-state_{controllerType.Name}.{member.Name}";
    }
}