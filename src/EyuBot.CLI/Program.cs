using System.CommandLine;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using EyuBot.CLI.Config;

namespace EyuBot.CLI
{
    class Program
    {
        private static HttpClient _httpClient;

        static async Task<int> Main(string[] args)
        {
            // Initialize HTTP client with token
            var configManager = new ConfigManager();
            var serverUrl = configManager.Configuration["Mcp:ServerUrl"] ?? "http://localhost:5000";
            var token = configManager.Configuration["Mcp:Token"];
            
            // Create HttpClientHandler to ignore SSL certificate errors (for development only)
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(serverUrl) };
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // 创建根命令
            var rootCommand = new RootCommand("EyuBot CLI工具");

            // 创建子命令
            var chatCommand = CreateChatCommand();
            var configCommand = CreateConfigCommand();
            var contextCommand = CreateContextCommand();
            var contextsCommand = CreateContextsCommand();
            var createCommand = CreateCreateCommand();
            var deleteCommand = CreateDeleteCommand();
            var statusCommand = CreateStatusCommand();

            // 添加子命令到根命令
            rootCommand.Subcommands.Add(chatCommand);
            rootCommand.Subcommands.Add(configCommand);
            rootCommand.Subcommands.Add(contextCommand);
            rootCommand.Subcommands.Add(contextsCommand);
            rootCommand.Subcommands.Add(createCommand);
            rootCommand.Subcommands.Add(deleteCommand);
            rootCommand.Subcommands.Add(statusCommand);

            // 执行命令
            return rootCommand.Parse(args).Invoke();
        }

        private static Command CreateChatCommand()
        {
            var command = new Command("chat", "发送消息并获取回复");
            var messageArgument = new Argument<string>("message")
            {
                Description = "要发送的消息"
            };
            command.Arguments.Add(messageArgument);

            command.SetAction(async (parseResult) =>
            {
                var message = parseResult.GetValue(messageArgument);
                Console.WriteLine($"发送消息: {message}");
                
                var request = new { Message = message, ContextId = "default" };
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/chat", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ChatResponse>(json);
                    if (result?.Success == true)
                    {
                        Console.WriteLine($"回复: {result.Response}");
                    }
                    else
                    {
                        Console.WriteLine($"错误: {result?.Error}");
                    }
                }
                else
                {
                    Console.WriteLine($"HTTP错误: {response.StatusCode}");
                }
                return 0;
            });

            return command;
        }

        private static Command CreateConfigCommand()
        {
            var command = new Command("config", "配置引导工具");

            command.SetAction((parseResult) =>
            {
                var configManager = new ConfigManager();
                var wizard = new ConfigWizard(configManager);
                wizard.Run();
                return 0;
            });

            return command;
        }

        private static Command CreateContextCommand()
        {
            var command = new Command("context", "切换对话上下文");
            var idArgument = new Argument<string>("id")
            {
                Description = "对话ID"
            };
            command.Arguments.Add(idArgument);

            command.SetAction(async (parseResult) =>
            {
                var id = parseResult.GetValue(idArgument);
                Console.WriteLine($"切换到对话: {id}");
                
                var request = new { ContextId = id };
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/context/switch", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ContextResponse>(json);
                    if (result?.Success == true)
                    {
                        Console.WriteLine($"对话加载成功，包含 {result.MessageCount} 条消息");
                    }
                    else
                    {
                        Console.WriteLine($"错误: {result?.Error}");
                    }
                }
                else
                {
                    Console.WriteLine($"HTTP错误: {response.StatusCode}");
                }
                return 0;
            });

            return command;
        }

        private static Command CreateContextsCommand()
        {
            var command = new Command("contexts", "列出所有对话上下文");

            command.SetAction(async (parseResult) =>
            {
                var response = await _httpClient.GetAsync("/api/contexts");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ContextListResponse>(json);
                    if (result?.Success == true)
                    {
                        Console.WriteLine("对话列表:");
                        int i = 1;
                        foreach (var contextId in result.ContextIds)
                        {
                            Console.WriteLine($"{i}. {contextId}");
                            i++;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"错误: {result?.Error}");
                    }
                }
                else
                {
                    Console.WriteLine($"HTTP错误: {response.StatusCode}");
                }
                return 0;
            });

            return command;
        }

        private static Command CreateCreateCommand()
        {
            var command = new Command("create", "创建新对话");

            command.SetAction(async (parseResult) =>
            {
                var response = await _httpClient.PostAsync("/api/context/create", null);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ContextResponse>(json);
                    if (result?.Success == true)
                    {
                        Console.WriteLine($"创建新对话: {result.ContextId}");
                    }
                    else
                    {
                        Console.WriteLine($"错误: {result?.Error}");
                    }
                }
                else
                {
                    Console.WriteLine($"HTTP错误: {response.StatusCode}");
                }
                return 0;
            });

            return command;
        }

        private static Command CreateDeleteCommand()
        {
            var command = new Command("delete", "删除对话");
            var idArgument = new Argument<string>("id")
            {
                Description = "对话ID"
            };
            command.Arguments.Add(idArgument);

            command.SetAction(async (parseResult) =>
            {
                var id = parseResult.GetValue(idArgument);
                var response = await _httpClient.DeleteAsync($"/api/context/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<DeleteResponse>(json);
                    if (result?.Success == true)
                    {
                        Console.WriteLine($"删除对话成功: {id}");
                    }
                    else
                    {
                        Console.WriteLine($"删除对话失败: {id}");
                        if (!string.IsNullOrEmpty(result?.Error))
                        {
                            Console.WriteLine($"错误: {result.Error}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"HTTP错误: {response.StatusCode}");
                }
                return 0;
            });

            return command;
        }

        private static Command CreateStatusCommand()
        {
            var command = new Command("status", "显示系统状态");

            command.SetAction(async (parseResult) =>
            {
                var response = await _httpClient.GetAsync("/api/status");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<StatusResponse>(json);
                    if (result?.Success == true)
                    {
                        Console.WriteLine("=== 系统状态 ===");
                        Console.WriteLine($"服务状态: {result.Status}");
                        Console.WriteLine($"存储类型: {result.StorageType}");
                        Console.WriteLine($"对话数量: {result.ConversationCount}");
                    }
                    else
                    {
                        Console.WriteLine($"错误: {result?.Error}");
                    }
                }
                else
                {
                    Console.WriteLine($"HTTP错误: {response.StatusCode}");
                }
                return 0;
            });

            return command;
        }

        // Response models
        private class ChatResponse
        {
            public string Response { get; set; }
            public bool Success { get; set; }
            public string Error { get; set; }
        }

        private class ContextResponse
        {
            public string ContextId { get; set; }
            public int MessageCount { get; set; }
            public bool Success { get; set; }
            public string Error { get; set; }
        }

        private class ContextListResponse
        {
            public string[] ContextIds { get; set; }
            public bool Success { get; set; }
            public string Error { get; set; }
        }

        private class DeleteResponse
        {
            public bool Success { get; set; }
            public string Error { get; set; }
        }

        private class StatusResponse
        {
            public string Status { get; set; }
            public string StorageType { get; set; }
            public int ConversationCount { get; set; }
            public bool Success { get; set; }
            public string Error { get; set; }
        }
    }
}
