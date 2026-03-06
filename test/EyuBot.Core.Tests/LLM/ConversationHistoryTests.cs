using Xunit;
using EyuBot.Core.LLM;

namespace EyuBot.Core.Tests.LLM
{
    public class ConversationHistoryTests
    {
        [Fact]
        public void AddMessage_AddsMessageToHistory()
        {
            // Arrange
            var history = new ConversationHistory();
            var message = new Message(MessageRole.User, "Test message");

            // Act
            history.AddMessage(message);

            // Assert
            Assert.Single(history.Messages);
            Assert.Equal(message, history.Messages[0]);
        }

        [Fact]
        public void AddUserMessage_AddsUserMessageToHistory()
        {
            // Arrange
            var history = new ConversationHistory();
            var content = "Test user message";

            // Act
            history.AddUserMessage(content);

            // Assert
            Assert.Single(history.Messages);
            Assert.Equal(MessageRole.User, history.Messages[0].Role);
            Assert.Equal(content, history.Messages[0].Content);
        }

        [Fact]
        public void AddAssistantMessage_AddsAssistantMessageToHistory()
        {
            // Arrange
            var history = new ConversationHistory();
            var content = "Test assistant message";

            // Act
            history.AddAssistantMessage(content);

            // Assert
            Assert.Single(history.Messages);
            Assert.Equal(MessageRole.Assistant, history.Messages[0].Role);
            Assert.Equal(content, history.Messages[0].Content);
        }

        [Fact]
        public void AddSystemMessage_AddsSystemMessageToHistory()
        {
            // Arrange
            var history = new ConversationHistory();
            var content = "Test system message";

            // Act
            history.AddSystemMessage(content);

            // Assert
            Assert.Single(history.Messages);
            Assert.Equal(MessageRole.System, history.Messages[0].Role);
            Assert.Equal(content, history.Messages[0].Content);
        }

        [Fact]
        public void Clear_RemovesAllMessages()
        {
            // Arrange
            var history = new ConversationHistory();
            history.AddUserMessage("Test message");

            // Act
            history.Clear();

            // Assert
            Assert.Empty(history.Messages);
        }

        [Fact]
        public void ToArray_ReturnsMessagesAsArray()
        {
            // Arrange
            var history = new ConversationHistory();
            history.AddUserMessage("Test message");

            // Act
            var messages = history.ToArray();

            // Assert
            Assert.Single(messages);
            Assert.Equal(MessageRole.User, messages[0].Role);
            Assert.Equal("Test message", messages[0].Content);
        }

        [Fact]
        public void AddMessage_RespectsMaxMessagesLimit()
        {
            // Arrange
            var maxMessages = 2;
            var history = new ConversationHistory(maxMessages);

            // Act
            history.AddUserMessage("Message 1");
            history.AddAssistantMessage("Message 2");
            history.AddUserMessage("Message 3"); // This should remove Message 1

            // Assert
            Assert.Equal(2, history.Messages.Count);
            Assert.Equal(MessageRole.Assistant, history.Messages[0].Role);
            Assert.Equal("Message 2", history.Messages[0].Content);
            Assert.Equal(MessageRole.User, history.Messages[1].Role);
            Assert.Equal("Message 3", history.Messages[1].Content);
        }
    }
}
