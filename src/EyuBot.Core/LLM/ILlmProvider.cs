using System.Threading.Tasks;

namespace EyuBot.Core.LLM
{
    /// <summary>
    /// Interface for LLM providers
    /// </summary>
    public interface ILlmProvider
    {
        /// <summary>
        /// Gets the provider name
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Sends a message to the LLM and gets a response
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns>The LLM response</returns>
        Task<string> GetResponseAsync(string message);

        /// <summary>
        /// Sends a message with conversation history to the LLM and gets a response
        /// </summary>
        /// <param name="messages">The conversation history</param>
        /// <returns>The LLM response</returns>
        Task<string> GetResponseAsync(Message[] messages);

        /// <summary>
        /// Sends a message to the LLM and gets a streaming response
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="onTokenReceived">Callback for each token received</param>
        /// <returns>The complete response</returns>
        Task<string> GetStreamingResponseAsync(string message, System.Action<string> onTokenReceived);

        /// <summary>
        /// Sends a message with conversation history to the LLM and gets a streaming response
        /// </summary>
        /// <param name="messages">The conversation history</param>
        /// <param name="onTokenReceived">Callback for each token received</param>
        /// <returns>The complete response</returns>
        Task<string> GetStreamingResponseAsync(Message[] messages, System.Action<string> onTokenReceived);
    }
}
