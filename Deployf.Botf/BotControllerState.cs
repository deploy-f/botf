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