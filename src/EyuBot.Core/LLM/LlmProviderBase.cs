using System.Threading.Tasks;

namespace EyuBot.Core.LLM
{
    /// <summary>
    /// Base class for LLM providers
    /// </summary>
    public abstract class LlmProviderBase : ILlmProvider
    {
        /// <summary>
        /// Gets the provider name
        /// </summary>
        public abstract string ProviderName { get; }

        /// <summary>
        /// Sends a message to the LLM and gets a response
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns>The LLM response</returns>
        public async Task<string> GetResponseAsync(string message)
        {
            var messages = new[] { new Message(MessageRole.User, message) };
            return await GetResponseAsync(messages);
        }

        /// <summary>
        /// Sends a message with conversation history to the LLM and gets a response
        /// </summary>
        /// <param name="messages">The conversation history</param>
        /// <returns>The LLM response</returns>
        public abstract Task<string> GetResponseAsync(Message[] messages);

        /// <summary>
        /// Sends a message to the LLM and gets a streaming response
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="onTokenReceived">Callback for each token received</param>
        /// <returns>The complete response</returns>
        public async Task<string> GetStreamingResponseAsync(string message, System.Action<string> onTokenReceived)
        {
            var messages = new[] { new Message(MessageRole.User, message) };
            return await GetStreamingResponseAsync(messages, onTokenReceived);
        }

        /// <summary>
        /// Sends a message with conversation history to the LLM and gets a streaming response
        /// </summary>
        /// <param name="messages">The conversation history</param>
        /// <param name="onTokenReceived">Callback for each token received</param>
        /// <returns>The complete response</returns>
        public abstract Task<string> GetStreamingResponseAsync(Message[] messages, System.Action<string> onTokenReceived);
    }
}
