namespace Deployf.Botf.Controllers
{
    public interface IChatFSM
    {
        bool ClearState(long? chatId);
        void Set(long? chatId, object state);
        object Get(long? chatId);
    }
}