#if NET5_0
    using ValueTask = System.Threading.Tasks.ValueTask;
    using ValueTaskGeneric = System.Threading.Tasks.ValueTask<object>;
#else
    using ValueTask = System.Threading.Tasks.Task;
    using ValueTaskGeneric = System.Threading.Tasks.Task<object>;
#endif


namespace Deployf.Botf;

public static class Consts
{
    public const string GLOBAL_STATE = "$gstate";
}

public abstract class BotControllerState : BotController
{
    public virtual ValueTask OnEnter() => ValueTask.CompletedTask;
    public virtual ValueTask OnLeave() => ValueTask.CompletedTask;
}

public abstract class BotControllerState<T> : BotControllerState
{
    public T? StateInstance { get; set; }

    public override async Task OnBeforeCall()
    {
        StateInstance = await Store!.Get<T>(FromId, Consts.GLOBAL_STATE, default(T));
        await base.OnBeforeCall();
    }
}