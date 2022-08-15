namespace Deployf.Botf;

public interface IKeyGenerator
{
    long GetLongId();
    Guid GetGuidId();
}

public class RandomKeyGenerator : IKeyGenerator
{
    public Guid GetGuidId()
    {
        return Guid.NewGuid();
    }

    public long GetLongId()
    {
        return Random.Shared.NextInt64();
    }
}