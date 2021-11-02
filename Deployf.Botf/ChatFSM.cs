using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Deployf.Botf.Controllers
{
    public class ChatFSM : IChatFSM
    {
        private ConcurrentDictionary<long, object> _state;
        private readonly ILogger<ChatFSM> _log;

        public ChatFSM(ILogger<ChatFSM> log)
        {
            _state = new ConcurrentDictionary<long, object>();
            _log = log;
        }

        public bool ClearState(long? chatId)
        {
            if (!chatId.HasValue)
            {
                return false;
            }

            if(_state.ContainsKey(chatId.Value))
            {
                _log.LogDebug("Remove state for {chatId}", chatId.Value);
                return _state.TryRemove(chatId.Value, out var _);
            }
            return false;
        }

        public object Get(long? chatId)
        {
            if (!chatId.HasValue
                || !_state.ContainsKey(chatId.Value)
                || !_state.TryGetValue(chatId.Value, out var state))
            {
                return null;
            }

            return state;
        }

        public void Set(long? chatId, object state)
        {
            if (!chatId.HasValue)
            {
                return;
            }

            _log.LogDebug("Set new state {@state} for {chatId}", state, chatId.Value);

            _state.AddOrUpdate(chatId.Value, state, (_, __) => state);
        }
    }
}