using Xunit;
using EyuBot.Core.LLM;

namespace EyuBot.Core.Tests.LLM
{
    public class ContextManagerTests
    {
        [Fact]
        public void GetOrCreateContext_CreatesNewContextWhenNotExists()
        {
            // Arrange
            var manager = new ContextManager();
            var contextId = "test-context";

            // Act
            var history = manager.GetOrCreateContext(contextId);

            // Assert
            Assert.NotNull(history);
            Assert.Equal(1, manager.ContextCount);
        }

        [Fact]
        public void GetOrCreateContext_ReturnsExistingContextWhenExists()
        {
            // Arrange
            var manager = new ContextManager();
            var contextId = "test-context";
            var firstHistory = manager.GetOrCreateContext(contextId);

            // Act
            var secondHistory = manager.GetOrCreateContext(contextId);

            // Assert
            Assert.Same(firstHistory, secondHistory);
            Assert.Equal(1, manager.ContextCount);
        }

        [Fact]
        public void RemoveContext_RemovesContext()
        {
            // Arrange
            var manager = new ContextManager();
            var contextId = "test-context";
            manager.GetOrCreateContext(contextId);

            // Act
            var result = manager.RemoveContext(contextId);

            // Assert
            Assert.True(result);
            Assert.Equal(0, manager.ContextCount);
        }

        [Fact]
        public void RemoveContext_ReturnsFalseWhenContextDoesNotExist()
        {
            // Arrange
            var manager = new ContextManager();
            var contextId = "non-existent-context";

            // Act
            var result = manager.RemoveContext(contextId);

            // Assert
            Assert.False(result);
            Assert.Equal(0, manager.ContextCount);
        }

        [Fact]
        public void ClearAllContexts_RemovesAllContexts()
        {
            // Arrange
            var manager = new ContextManager();
            manager.GetOrCreateContext("context1");
            manager.GetOrCreateContext("context2");

            // Act
            manager.ClearAllContexts();

            // Assert
            Assert.Equal(0, manager.ContextCount);
        }

        [Fact]
        public void ContextCount_ReturnsCorrectCount()
        {
            // Arrange
            var manager = new ContextManager();

            // Act & Assert
            Assert.Equal(0, manager.ContextCount);
            
            manager.GetOrCreateContext("context1");
            Assert.Equal(1, manager.ContextCount);
            
            manager.GetOrCreateContext("context2");
            Assert.Equal(2, manager.ContextCount);
            
            manager.RemoveContext("context1");
            Assert.Equal(1, manager.ContextCount);
        }
    }
}
