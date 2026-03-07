using Microsoft.AspNetCore.Mvc;
using EyuBot.Storage;
using EyuBot.CLI.Config;
using EyuBot.Abstractions;
using EyuBot.Core.LLM;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace EyuBot.App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContextController : ControllerBase
    {
        [HttpPost("switch")]
        public async Task<ActionResult<ContextResponse>> Switch([FromBody] ContextRequest request)
        {
            try
            {
                var configManager = new ConfigManager();
                IStorage storage = null;
                try
                {
                    storage = CreateStorage(configManager);
                    if (storage != null)
                    {
                        await storage.InitializeAsync();
                    }
                }
                catch (Exception storageEx)
                {
                    Console.WriteLine($"Storage initialization error: {storageEx.Message}");
                    // 存储初始化失败，继续执行，使用内存存储
                }

                var contextManager = new ContextManager(50, storage);
                object history = null;
                try
                {
                    history = await contextManager.GetOrCreateContextAsync(request.ContextId);
                }
                catch (Exception getEx)
                {
                    Console.WriteLine($"Get context error: {getEx.Message}");
                    // 获取对话失败，使用null，MessageCount设为0
                }

                int messageCount = 0;
                if (history != null)
                {
                    var messagesProperty = history.GetType().GetProperty("Messages");
                    if (messagesProperty != null)
                    {
                        var messages = messagesProperty.GetValue(history);
                        if (messages != null)
                        {
                            messageCount = ((System.Collections.IEnumerable)messages).Cast<object>().Count();
                        }
                    }
                }

                return Ok(new ContextResponse { ContextId = request.ContextId, MessageCount = messageCount, Success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest(new ContextResponse { Success = false, Error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<ContextListResponse>> Get()
        {
            try
            {
                var configManager = new ConfigManager();
                IStorage storage = null;
                try
                {
                    storage = CreateStorage(configManager);
                    if (storage != null)
                    {
                        await storage.InitializeAsync();
                    }
                }
                catch (Exception storageEx)
                {
                    Console.WriteLine($"Storage initialization error: {storageEx.Message}");
                    // 存储初始化失败，继续执行，使用内存存储
                }

                var contextManager = new ContextManager(50, storage);
                IEnumerable<string> conversations = new List<string>();
                try
                {
                    conversations = await contextManager.ListConversationsAsync();
                }
                catch (Exception listEx)
                {
                    Console.WriteLine($"List conversations error: {listEx.Message}");
                    // 列出对话失败，使用空列表
                }

                return Ok(new ContextListResponse { ContextIds = conversations.ToArray(), Success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest(new ContextListResponse { Success = false, Error = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<ActionResult<ContextResponse>> Create()
        {
            try
            {
                var conversationId = Guid.NewGuid().ToString();
                return Ok(new ContextResponse { ContextId = conversationId, MessageCount = 0, Success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new ContextResponse { Success = false, Error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<DeleteResponse>> Delete(string id)
        {
            try
            {
                var configManager = new ConfigManager();
                IStorage storage = null;
                try
                {
                    storage = CreateStorage(configManager);
                    if (storage != null)
                    {
                        await storage.InitializeAsync();
                    }
                }
                catch (Exception storageEx)
                {
                    Console.WriteLine($"Storage initialization error: {storageEx.Message}");
                    // 存储初始化失败，继续执行，使用内存存储
                }

                var contextManager = new ContextManager(50, storage);
                var result = await contextManager.RemoveContextAsync(id);

                return Ok(new DeleteResponse { Success = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest(new DeleteResponse { Success = false, Error = ex.Message });
            }
        }

        private static IStorage CreateStorage(ConfigManager configManager)
        {
            var storageType = configManager.Configuration["Storage:Type"] ?? "Sqlite";
            var connectionString = configManager.Configuration["Storage:Sqlite:ConnectionString"] ?? "Data Source=eyubot.db";

            try
            {
                return StorageFactory.CreateStorage(storageType, connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating storage: {ex.Message}");
                return null;
            }
        }
    }

    // Request and response models
    public class ContextRequest
    {
        public string ContextId { get; set; }
    }

    public class ContextResponse
    {
        public string ContextId { get; set; }
        public int MessageCount { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    public class ContextListResponse
    {
        public string[] ContextIds { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    public class DeleteResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}