namespace Deployf.Botf;

public interface IGlobalStateService
{
    Task SetState<T>(long userId, T newState, bool callEnter = true, bool callLeave = true, CancellationToken cancelToken = default);
}