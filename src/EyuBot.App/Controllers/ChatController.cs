using Microsoft.AspNetCore.Mvc;
using EyuBot.Storage;
using EyuBot.CLI.Config;
using EyuBot.Abstractions;
using EyuBot.Core.LLM;
using System;
using System.Threading.Tasks;

namespace EyuBot.App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request)
        {
            try
            {
                var configManager = new ConfigManager();
                Console.WriteLine($"ConfigPath: {System.IO.Path.Combine(AppContext.BaseDirectory, "appsettings.json")}");
                Console.WriteLine($"StorageType: {configManager.Configuration["Storage:Type"]}");
                Console.WriteLine($"ConnectionString: {configManager.Configuration["Storage:Sqlite:ConnectionString"]}");
                Console.WriteLine($"Stream: {request.Stream}");
                
                IStorage storage = null;
                try
                {
                    storage = CreateStorage(configManager);
                    Console.WriteLine($"Storage created: {storage != null}");
                    
                    if (storage != null)
                    {
                        await storage.InitializeAsync();
                        Console.WriteLine("Storage initialized");
                    }
                }
                catch (Exception storageEx)
                {
                    Console.WriteLine($"Storage initialization error: {storageEx.Message}");
                    // 存储初始化失败，继续执行，使用内存存储
                }

                var contextManager = new ContextManager(50, storage);
                Console.WriteLine("ContextManager created");
                
                var history = await contextManager.GetOrCreateContextAsync(request.ContextId ?? "default");
                Console.WriteLine("History retrieved");

                // Get response from LLM (simulated for now)
                await Task.Delay(1000);
                var response = "This is a test response: " + request.Message;
                Console.WriteLine($"Response generated: {response}");

                // Save conversation
                history.AddUserMessage(request.Message);
                history.AddAssistantMessage(response);
                try
                {
                    await contextManager.SaveContextAsync(request.ContextId ?? "default");
                    Console.WriteLine("Conversation saved");
                }
                catch (Exception saveEx)
                {
                    Console.WriteLine($"Save conversation error: {saveEx.Message}");
                    // 保存失败，继续执行
                }

                if (request.Stream)
                {
                    // 流式响应
                    Response.ContentType = "text/event-stream";
                    Response.Headers.Append("Cache-Control", "no-cache");
                    Response.Headers.Append("Connection", "keep-alive");

                    // 模拟流式输出
                    var words = response.Split(' ');
                    foreach (var word in words)
                    {
                        await Response.WriteAsync($"data: {word} \n\n");
                        await Response.Body.FlushAsync();
                        await Task.Delay(100);
                    }

                    // 结束流
                    await Response.WriteAsync("data: [DONE]\n\n");
                    await Response.Body.FlushAsync();

                    return new EmptyResult();
                }
                else
                {
                    // 普通响应
                    return Ok(new ChatResponse { Response = response, Success = true });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest(new ChatResponse { Success = false, Error = ex.Message });
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
    public class ChatRequest
    {
        public string Message { get; set; }
        public string ContextId { get; set; }
        public bool Stream { get; set; } = false;
    }

    public class ChatResponse
    {
        public string Response { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}