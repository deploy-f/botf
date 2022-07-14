namespace Deployf.Botf;

public class FlagMessageBuilder<T> where T : struct, Enum
{
    private readonly T _value;
    private Func<T, string>? _navigation;

    public FlagMessageBuilder(T value)
    {
        _value = value;
    }

    public FlagMessageBuilder<T> Navigation(Func<T, string> nav)
    {
        _navigation = nav;
        return this;
    }

    public void Build(MessageBuilder b)
    {
        var flags = Enum.GetNames(typeof(T));
        var values = Enum.GetValues(typeof(T));

        for (int i = 0; i < values.Length; i++)
        {
            T value = (T)values.GetValue(i)!;
            var isSet = _value.HasFlag(value);

            var title = value.ToString();
            if (isSet)
            {
                title = "🔘" + title;
            }

            b.RowButton(title, _navigation!(Enum.Parse<T>((Convert.ToInt32(_value) ^ Convert.ToInt32(value)).ToString())));
        }
    }
}