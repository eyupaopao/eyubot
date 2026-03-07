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
    public class StatusController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<StatusResponse>> Get()
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

                return Ok(new StatusResponse
                {
                    Status = "Running",
                    StorageType = configManager.Configuration["Storage:Type"] ?? "Sqlite",
                    ConversationCount = conversations.Count(),
                    Success = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest(new StatusResponse { Success = false, Error = ex.Message });
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

    // Response model
    public class StatusResponse
    {
        public string Status { get; set; }
        public string StorageType { get; set; }
        public int ConversationCount { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}