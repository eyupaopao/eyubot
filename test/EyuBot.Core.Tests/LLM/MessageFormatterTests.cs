using Xunit;
using EyuBot.Core.LLM;

namespace EyuBot.Core.Tests.LLM
{
    public class MessageFormatterTests
    {
        [Fact]
        public void FormatSystemMessage_ReturnsSystemMessage()
        {
            // Arrange
            var content = "Test system message";

            // Act
            var message = MessageFormatter.FormatSystemMessage(content);

            // Assert
            Assert.Equal(MessageRole.System, message.Role);
            Assert.Equal(content, message.Content);
        }

        [Fact]
        public void FormatUserMessage_ReturnsUserMessage()
        {
            // Arrange
            var content = "Test user message";

            // Act
            var message = MessageFormatter.FormatUserMessage(content);

            // Assert
            Assert.Equal(MessageRole.User, message.Role);
            Assert.Equal(content, message.Content);
        }

        [Fact]
        public void FormatAssistantMessage_ReturnsAssistantMessage()
        {
            // Arrange
            var content = "Test assistant message";

            // Act
            var message = MessageFormatter.FormatAssistantMessage(content);

            // Assert
            Assert.Equal(MessageRole.Assistant, message.Role);
            Assert.Equal(content, message.Content);
        }

        [Fact]
        public void FormatConversation_WithUserMessageOnly_ReturnsSingleUserMessage()
        {
            // Arrange
            var userMessage = "Test user message";

            // Act
            var messages = MessageFormatter.FormatConversation(userMessage);

            // Assert
            Assert.Single(messages);
            Assert.Equal(MessageRole.User, messages[0].Role);
            Assert.Equal(userMessage, messages[0].Content);
        }

        [Fact]
        public void FormatConversation_WithSystemPrompt_ReturnsSystemAndUserMessages()
        {
            // Arrange
            var userMessage = "Test user message";
            var systemPrompt = "Test system prompt";

            // Act
            var messages = MessageFormatter.FormatConversation(userMessage, systemPrompt);

            // Assert
            Assert.Equal(2, messages.Length);
            Assert.Equal(MessageRole.System, messages[0].Role);
            Assert.Equal(systemPrompt, messages[0].Content);
            Assert.Equal(MessageRole.User, messages[1].Role);
            Assert.Equal(userMessage, messages[1].Content);
        }

        [Fact]
        public void FormatConversation_WithHistory_ReturnsHistoryAndNewUserMessage()
        {
            // Arrange
            var userMessage = "New user message";
            var history = new ConversationHistory();
            history.AddUserMessage("Previous user message");
            history.AddAssistantMessage("Previous assistant message");

            // Act
            var messages = MessageFormatter.FormatConversation(userMessage, history);

            // Assert
            Assert.Equal(3, messages.Length);
            Assert.Equal(MessageRole.User, messages[0].Role);
            Assert.Equal("Previous user message", messages[0].Content);
            Assert.Equal(MessageRole.Assistant, messages[1].Role);
            Assert.Equal("Previous assistant message", messages[1].Content);
            Assert.Equal(MessageRole.User, messages[2].Role);
            Assert.Equal(userMessage, messages[2].Content);
        }

        [Fact]
        public void FormatConversation_WithHistoryAndSystemPrompt_ReturnsSystemPromptHistoryAndNewUserMessage()
        {
            // Arrange
            var userMessage = "New user message";
            var systemPrompt = "Test system prompt";
            var history = new ConversationHistory();
            history.AddUserMessage("Previous user message");

            // Act
            var messages = MessageFormatter.FormatConversation(userMessage, history, systemPrompt);

            // Assert
            Assert.Equal(3, messages.Length);
            Assert.Equal(MessageRole.System, messages[0].Role);
            Assert.Equal(systemPrompt, messages[0].Content);
            Assert.Equal(MessageRole.User, messages[1].Role);
            Assert.Equal("Previous user message", messages[1].Content);
            Assert.Equal(MessageRole.User, messages[2].Role);
            Assert.Equal(userMessage, messages[2].Content);
        }
    }
}
