namespace EyuBot.Abstractions
{
    public interface IStorage
    {
        System.Threading.Tasks.Task InitializeAsync();
        System.Threading.Tasks.Task SaveConversationAsync(string conversationId, object history);
        System.Threading.Tasks.Task<object> LoadConversationAsync(string conversationId);
        System.Threading.Tasks.Task DeleteConversationAsync(string conversationId);
        System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<string>> ListConversationsAsync();
    }
}
