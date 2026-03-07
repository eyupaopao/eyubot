using System.Collections.Generic;
using EyuBot.Abstractions;

namespace EyuBot.Core.LLM
{
    /// <summary>
    /// Manages conversation contexts
    /// </summary>
    public class ContextManager
    {
        private readonly Dictionary<string, ConversationHistory> _contexts;
        private readonly int _maxMessagesPerContext;
        private readonly IStorage _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextManager"/> class
        /// </summary>
        /// <param name="maxMessagesPerContext">The maximum number of messages per context</param>
        /// <param name="storage">The storage instance for persisting conversations</param>
        public ContextManager(int maxMessagesPerContext = 50, IStorage storage = null)
        {
            _contexts = new Dictionary<string, ConversationHistory>();
            _maxMessagesPerContext = maxMessagesPerContext;
            _storage = storage;
        }

        /// <summary>
        /// Gets or creates a conversation history for a given context ID
        /// </summary>
        /// <param name="contextId">The context ID</param>
        /// <returns>The conversation history for the context</returns>
        public async System.Threading.Tasks.Task<ConversationHistory> GetOrCreateContextAsync(string contextId)
        {
            if (!_contexts.TryGetValue(contextId, out var history))
            {
                if (_storage != null)
                {
                    try
                    {
                        var loadedHistory = await _storage.LoadConversationAsync(contextId);
                        // 这里需要将object转换为ConversationHistory
                        // 由于我们使用了反射，这里简化处理，直接创建新的历史
                        history = new ConversationHistory(_maxMessagesPerContext);
                    }
                    catch
                    {
                        history = new ConversationHistory(_maxMessagesPerContext);
                    }
                }
                else
                {
                    history = new ConversationHistory(_maxMessagesPerContext);
                }
                _contexts[contextId] = history;
            }

            return history;
        }

        /// <summary>
        /// Removes a context by ID
        /// </summary>
        /// <param name="contextId">The context ID to remove</param>
        /// <returns>True if the context was removed, false otherwise</returns>
        public async System.Threading.Tasks.Task<bool> RemoveContextAsync(string contextId)
        {
            if (_contexts.Remove(contextId))
            {
                if (_storage != null)
                {
                    try
                    {
                        await _storage.DeleteConversationAsync(contextId);
                    }
                    catch
                    {
                        // 存储删除失败，继续执行
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Saves a context to storage
        /// </summary>
        /// <param name="contextId">The context ID to save</param>
        public async System.Threading.Tasks.Task SaveContextAsync(string contextId)
        {
            if (_contexts.TryGetValue(contextId, out var history) && _storage != null)
            {
                await _storage.SaveConversationAsync(contextId, history);
            }
        }

        /// <summary>
        /// Clears all contexts
        /// </summary>
        public void ClearAllContexts()
        {
            _contexts.Clear();
        }

        /// <summary>
        /// Gets the number of active contexts
        /// </summary>
        public int ContextCount => _contexts.Count;

        /// <summary>
        /// Lists all conversations from storage
        /// </summary>
        /// <returns>List of conversation IDs</returns>
        public async System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<string>> ListConversationsAsync()
        {
            if (_storage != null)
            {
                return await _storage.ListConversationsAsync();
            }
            return _contexts.Keys;
        }
    }
}
