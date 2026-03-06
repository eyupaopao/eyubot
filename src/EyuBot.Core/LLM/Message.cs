namespace EyuBot.Core.LLM
{
    /// <summary>
    /// Represents a message in a conversation
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the role of the message sender
        /// </summary>
        public MessageRole Role { get; set; }

        /// <summary>
        /// Gets or sets the content of the message
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class
        /// </summary>
        /// <param name="role">The role of the message sender</param>
        /// <param name="content">The content of the message</param>
        public Message(MessageRole role, string content)
        {
            Role = role;
            Content = content;
        }
    }

    /// <summary>
    /// Represents the role of a message sender
    /// </summary>
    public enum MessageRole
    {
        /// <summary>
        /// System message
        /// </summary>
        System,

        /// <summary>
        /// User message
        /// </summary>
        User,

        /// <summary>
        /// Assistant message
        /// </summary>
        Assistant
    }
}
