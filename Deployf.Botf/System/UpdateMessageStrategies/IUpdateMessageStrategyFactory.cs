namespace Deployf.Botf.System.UpdateMessageStrategies;

public interface IUpdateMessageStrategyFactory
{
    IUpdateMessageStrategy? GetStrategy(IUpdateMessageContext context);
}

public class UpdateMessageStrategyFactory : IUpdateMessageStrategyFactory
{
    private readonly IEnumerable<IUpdateMessageStrategy> _strategies;

    public UpdateMessageStrategyFactory(IEnumerable<IUpdateMessageStrategy> strategies)
    {
        _strategies = strategies;
    }
    
    public IUpdateMessageStrategy? GetStrategy(IUpdateMessageContext context)
    {
        return _strategies.FirstOrDefault(s => s.CanHandle(context));
    }
}