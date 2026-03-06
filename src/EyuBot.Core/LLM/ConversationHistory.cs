using System.Collections.Generic;

namespace EyuBot.Core.LLM
{
    /// <summary>
    /// Manages conversation history
    /// </summary>
    public class ConversationHistory
    {
        private readonly List<Message> _messages;
        private readonly int _maxMessages;

        /// <summary>
        /// Gets the current messages in the conversation
        /// </summary>
        public IReadOnlyList<Message> Messages => _messages;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationHistory"/> class
        /// </summary>
        /// <param name="maxMessages">The maximum number of messages to keep</param>
        public ConversationHistory(int maxMessages = 50)
        {
            _messages = new List<Message>();
            _maxMessages = maxMessages;
        }

        /// <summary>
        /// Adds a message to the conversation history
        /// </summary>
        /// <param name="message">The message to add</param>
        public void AddMessage(Message message)
        {
            _messages.Add(message);
            
            // Remove oldest messages if exceeding limit
            while (_messages.Count > _maxMessages)
            {
                _messages.RemoveAt(0);
            }
        }

        /// <summary>
        /// Adds a user message to the conversation history
        /// </summary>
        /// <param name="content">The content of the message</param>
        public void AddUserMessage(string content)
        {
            AddMessage(new Message(MessageRole.User, content));
        }

        /// <summary>
        /// Adds an assistant message to the conversation history
        /// </summary>
        /// <param name="content">The content of the message</param>
        public void AddAssistantMessage(string content)
        {
            AddMessage(new Message(MessageRole.Assistant, content));
        }

        /// <summary>
        /// Adds a system message to the conversation history
        /// </summary>
        /// <param name="content">The content of the message</param>
        public void AddSystemMessage(string content)
        {
            AddMessage(new Message(MessageRole.System, content));
        }

        /// <summary>
        /// Clears the conversation history
        /// </summary>
        public void Clear()
        {
            _messages.Clear();
        }

        /// <summary>
        /// Gets the conversation history as an array of messages
        /// </summary>
        /// <returns>An array of messages</returns>
        public Message[] ToArray()
        {
            return _messages.ToArray();
        }
    }
}
