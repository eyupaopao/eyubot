using System.Data.SQLite;
using System.Data;
using Newtonsoft.Json;
using EyuBot.Abstractions;

namespace EyuBot.Storage
{
    public class SqliteStorage : IStorage
    {
        private readonly string _connectionString;

        public SqliteStorage(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            // 创建对话表
            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS conversations (
                    id TEXT PRIMARY KEY,
                    name TEXT,
                    created_at TEXT,
                    updated_at TEXT,
                    metadata TEXT
                );

                CREATE TABLE IF NOT EXISTS messages (
                    id TEXT PRIMARY KEY,
                    conversation_id TEXT,
                    role INTEGER,
                    content TEXT,
                    timestamp TEXT,
                    FOREIGN KEY (conversation_id) REFERENCES conversations(id) ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS idx_messages_conversation_id ON messages(conversation_id);
            ";
            await command.ExecuteNonQueryAsync();
        }

        public async System.Threading.Tasks.Task SaveConversationAsync(string conversationId, object history)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 保存对话元数据
                using var conversationCommand = connection.CreateCommand();
                conversationCommand.CommandText = @"
                    INSERT OR REPLACE INTO conversations (id, name, created_at, updated_at, metadata)
                    VALUES (@id, @name, @created_at, @updated_at, @metadata)
                ";
                conversationCommand.Parameters.AddWithValue("@id", conversationId);
                conversationCommand.Parameters.AddWithValue("@name", "Conversation " + conversationId);
                conversationCommand.Parameters.AddWithValue("@created_at", System.DateTime.UtcNow.ToString());
                conversationCommand.Parameters.AddWithValue("@updated_at", System.DateTime.UtcNow.ToString());
                conversationCommand.Parameters.AddWithValue("@metadata", JsonConvert.SerializeObject(new { }));
                await conversationCommand.ExecuteNonQueryAsync();

                // 删除旧消息
                using var deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText = "DELETE FROM messages WHERE conversation_id = @conversation_id";
                deleteCommand.Parameters.AddWithValue("@conversation_id", conversationId);
                await deleteCommand.ExecuteNonQueryAsync();

                // 保存新消息
                var historyType = history.GetType();
                var messagesProperty = historyType.GetProperty("Messages");
                if (messagesProperty != null)
                {
                    var messages = messagesProperty.GetValue(history) as System.Collections.IEnumerable;
                    if (messages != null)
                    {
                        foreach (var message in messages)
                        {
                            var messageType = message.GetType();
                            var roleProperty = messageType.GetProperty("Role");
                            var contentProperty = messageType.GetProperty("Content");
                            
                            if (roleProperty != null && contentProperty != null)
                            {
                                var role = (int)roleProperty.GetValue(message);
                                var content = contentProperty.GetValue(message) as string;
                                
                                using var messageCommand = connection.CreateCommand();
                                messageCommand.CommandText = @"
                                    INSERT INTO messages (id, conversation_id, role, content, timestamp)
                                    VALUES (@id, @conversation_id, @role, @content, @timestamp)
                                ";
                                messageCommand.Parameters.AddWithValue("@id", System.Guid.NewGuid().ToString());
                                messageCommand.Parameters.AddWithValue("@conversation_id", conversationId);
                                messageCommand.Parameters.AddWithValue("@role", role);
                                messageCommand.Parameters.AddWithValue("@content", content);
                                messageCommand.Parameters.AddWithValue("@timestamp", System.DateTime.UtcNow.ToString());
                                await messageCommand.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async System.Threading.Tasks.Task<object> LoadConversationAsync(string conversationId)
        {
            // 这里需要动态创建ConversationHistory对象
            // 由于我们不能直接引用EyuBot.Core，所以返回object
            // 实际使用时需要进行类型转换
            return new { Messages = new System.Collections.ArrayList() };
        }

        public async System.Threading.Tasks.Task DeleteConversationAsync(string conversationId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM conversations WHERE id = @id";
            command.Parameters.AddWithValue("@id", conversationId);
            await command.ExecuteNonQueryAsync();
        }

        public async System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<string>> ListConversationsAsync()
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            var conversations = new System.Collections.Generic.List<string>();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT id FROM conversations ORDER BY updated_at DESC";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                conversations.Add(reader.GetString(0));
            }

            return conversations;
        }
    }
}
