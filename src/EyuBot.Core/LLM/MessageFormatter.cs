using System.Collections.Generic;

namespace EyuBot.Core.LLM
{
    /// <summary>
    /// Formats messages for LLM providers
    /// </summary>
    public static class MessageFormatter
    {
        /// <summary>
        /// Formats a system message
        /// </summary>
        /// <param name="content">The content of the system message</param>
        /// <returns>A formatted system message</returns>
        public static Message FormatSystemMessage(string content)
        {
            return new Message(MessageRole.System, content);
        }

        /// <summary>
        /// Formats a user message
        /// </summary>
        /// <param name="content">The content of the user message</param>
        /// <returns>A formatted user message</returns>
        public static Message FormatUserMessage(string content)
        {
            return new Message(MessageRole.User, content);
        }

        /// <summary>
        /// Formats an assistant message
        /// </summary>
        /// <param name="content">The content of the assistant message</param>
        /// <returns>A formatted assistant message</returns>
        public static Message FormatAssistantMessage(string content)
        {
            return new Message(MessageRole.Assistant, content);
        }

        /// <summary>
        /// Formats a conversation prompt with context
        /// </summary>
        /// <param name="userMessage">The user message</param>
        /// <param name="systemPrompt">The system prompt</param>
        /// <returns>An array of formatted messages</returns>
        public static Message[] FormatConversation(string userMessage, string systemPrompt = null)
        {
            var messages = new List<Message>();

            if (!string.IsNullOrEmpty(systemPrompt))
            {
                messages.Add(FormatSystemMessage(systemPrompt));
            }

            messages.Add(FormatUserMessage(userMessage));

            return messages.ToArray();
        }

        /// <summary>
        /// Formats a conversation prompt with history
        /// </summary>
        /// <param name="userMessage">The user message</param>
        /// <param name="history">The conversation history</param>
        /// <param name="systemPrompt">The system prompt</param>
        /// <returns>An array of formatted messages</returns>
        public static Message[] FormatConversation(string userMessage, ConversationHistory history, string systemPrompt = null)
        {
            var messages = new List<Message>();

            if (!string.IsNullOrEmpty(systemPrompt))
            {
                messages.Add(FormatSystemMessage(systemPrompt));
            }

            // Add existing history
            messages.AddRange(history.Messages);

            // Add new user message
            var userMsg = FormatUserMessage(userMessage);
            messages.Add(userMsg);

            return messages.ToArray();
        }
    }
}
