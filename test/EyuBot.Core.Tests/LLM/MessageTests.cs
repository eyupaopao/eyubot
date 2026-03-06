using Xunit;
using EyuBot.Core.LLM;

namespace EyuBot.Core.Tests.LLM
{
    public class MessageTests
    {
        [Fact]
        public void Message_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var role = MessageRole.User;
            var content = "Test message";

            // Act
            var message = new Message(role, content);

            // Assert
            Assert.Equal(role, message.Role);
            Assert.Equal(content, message.Content);
        }

        [Fact]
        public void MessageRole_Values_AreCorrect()
        {
            // Assert
            Assert.Equal(0, (int)MessageRole.System);
            Assert.Equal(1, (int)MessageRole.User);
            Assert.Equal(2, (int)MessageRole.Assistant);
        }
    }
}
