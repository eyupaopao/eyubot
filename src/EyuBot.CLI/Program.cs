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
            var serverUrl = "http://localhost:64400"; // 强制使用HTTP连接
            var token = string.IsNullOrEmpty(configManager.Configuration["Mcp:Token"]) ? "eyubot-secret-token-2026" : configManager.Configuration["Mcp:Token"]; // 使用与服务器相同的令牌
            
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
            var helpCommand = CreateHelpCommand();
            var versionCommand = CreateVersionCommand();
            var aboutCommand = CreateAboutCommand();

            // 添加子命令到根命令
            rootCommand.Subcommands.Add(chatCommand);
            rootCommand.Subcommands.Add(configCommand);
            rootCommand.Subcommands.Add(contextCommand);
            rootCommand.Subcommands.Add(contextsCommand);
            rootCommand.Subcommands.Add(createCommand);
            rootCommand.Subcommands.Add(deleteCommand);
            rootCommand.Subcommands.Add(statusCommand);
            rootCommand.Subcommands.Add(helpCommand);
            rootCommand.Subcommands.Add(versionCommand);
            rootCommand.Subcommands.Add(aboutCommand);

            // 执行命令
            return rootCommand.Parse(args).Invoke();
        }

        private static Command CreateChatCommand()
        {
            var command = new Command("chat", "启动对话交互");

            var contextOption = new Option<string>("--context", "对话ID");
            var messageOption = new Option<string>("--message", "直接发送消息");
            var streamOption = new Option<bool>("--stream", "启用流式响应");
            
            command.Options.Add(contextOption);
            command.Options.Add(messageOption);
            command.Options.Add(streamOption);

            command.SetAction(async (parseResult) =>
            {
                var contextId = parseResult.GetValue(contextOption);
                var message = parseResult.GetValue(messageOption);
                var stream = parseResult.GetValue(streamOption);

                // 如果没有指定对话ID，列出所有对话供选择
                if (string.IsNullOrEmpty(contextId))
                {
                    Console.WriteLine("正在获取对话列表...");
                    var contextsResponse = await _httpClient.GetAsync("/api/context");
                    if (contextsResponse.IsSuccessStatusCode)
                    {
                        var contextsJson = await contextsResponse.Content.ReadAsStringAsync();
                        var contextsResult = JsonConvert.DeserializeObject<ContextListResponse>(contextsJson);
                        if (contextsResult?.Success == true && contextsResult.ContextIds.Length > 0)
                        {
                            Console.WriteLine("可用的对话:");
                            for (int i = 0; i < contextsResult.ContextIds.Length; i++)
                            {
                                Console.WriteLine($"{i + 1}. {contextsResult.ContextIds[i]}");
                            }
                            Console.WriteLine($"{contextsResult.ContextIds.Length + 1}. 创建新对话");
                            Console.WriteLine($"{contextsResult.ContextIds.Length + 2}. 删除对话");
                            
                            Console.Write("请选择操作编号: ");
                            var input = Console.ReadLine();
                            if (int.TryParse(input, out int choice))
                            {
                                if (choice > 0 && choice <= contextsResult.ContextIds.Length)
                                {
                                    contextId = contextsResult.ContextIds[choice - 1];
                                    Console.WriteLine($"选择对话: {contextId}");
                                }
                                else if (choice == contextsResult.ContextIds.Length + 1)
                                {
                                    // 创建新对话
                                    var createResponse = await _httpClient.PostAsync("/api/context/create", null);
                                    if (createResponse.IsSuccessStatusCode)
                                    {
                                        var createJson = await createResponse.Content.ReadAsStringAsync();
                                        var createResult = JsonConvert.DeserializeObject<ContextResponse>(createJson);
                                        if (createResult?.Success == true)
                                        {
                                            contextId = createResult.ContextId;
                                            Console.WriteLine($"创建新对话: {contextId}");
                                        }
                                    }
                                }
                                else if (choice == contextsResult.ContextIds.Length + 2)
                                {
                                    // 删除对话
                                    Console.Write("请输入要删除的对话ID: ");
                                    var deleteId = Console.ReadLine();
                                    if (!string.IsNullOrEmpty(deleteId))
                                    {
                                        var deleteResponse = await _httpClient.DeleteAsync($"/api/context/{deleteId}");
                                        if (deleteResponse.IsSuccessStatusCode)
                                        {
                                            var deleteJson = await deleteResponse.Content.ReadAsStringAsync();
                                            var deleteResult = JsonConvert.DeserializeObject<DeleteResponse>(deleteJson);
                                            if (deleteResult?.Success == true)
                                            {
                                                Console.WriteLine($"删除对话成功: {deleteId}");
                                            }
                                            else
                                            {
                                                Console.WriteLine($"删除对话失败: {deleteId}");
                                                if (!string.IsNullOrEmpty(deleteResult?.Error))
                                                {
                                                    Console.WriteLine($"错误: {deleteResult.Error}");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine($"HTTP错误: {deleteResponse.StatusCode}");
                                        }
                                    }
                                    return 0;
                                }
                            }
                        }
                        else
                        {
                            // 没有对话，创建新对话
                            var createResponse = await _httpClient.PostAsync("/api/context/create", null);
                            if (createResponse.IsSuccessStatusCode)
                            {
                                var createJson = await createResponse.Content.ReadAsStringAsync();
                                var createResult = JsonConvert.DeserializeObject<ContextResponse>(createJson);
                                if (createResult?.Success == true)
                                {
                                    contextId = createResult.ContextId;
                                    Console.WriteLine($"创建新对话: {contextId}");
                                }
                            }
                        }
                    }
                }

                // 如果仍然没有对话ID，使用默认值
                if (string.IsNullOrEmpty(contextId))
                {
                    contextId = "default";
                }

                // 如果指定了消息，直接发送
                if (!string.IsNullOrEmpty(message))
                {
                    var request = new { Message = message, ContextId = contextId, Stream = stream };
                    var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("/api/chat", content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ChatResponse>(json);
                        if (result?.Success == true)
                        {
                            Console.WriteLine($"AI: {result.Response}");
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
                }

                // 进入对话循环
                Console.WriteLine($"进入对话模式 (对话ID: {contextId})");
                Console.WriteLine("输入 'exit' 退出对话");
                Console.WriteLine("输入 'clear' 清空屏幕");
                Console.WriteLine("输入 'help' 显示帮助信息");
                Console.WriteLine("输入 'context' 显示当前对话ID");
                Console.WriteLine("输入 'switch' 切换对话");
                
                while (true)
                {
                    Console.Write("你: ");
                    var userInput = Console.ReadLine();
                    
                    if (userInput == "exit")
                    {
                        break;
                    }
                    
                    if (userInput == "clear")
                    {
                        Console.Clear();
                        Console.WriteLine($"对话模式 (对话ID: {contextId})");
                        Console.WriteLine("输入 'exit' 退出对话");
                        Console.WriteLine("输入 'clear' 清空屏幕");
                        Console.WriteLine("输入 'help' 显示帮助信息");
                        Console.WriteLine("输入 'context' 显示当前对话ID");
                        Console.WriteLine("输入 'switch' 切换对话");
                        continue;
                    }
                    
                    if (userInput == "help")
                    {
                        Console.WriteLine("命令帮助:");
                        Console.WriteLine("  exit - 退出对话");
                        Console.WriteLine("  clear - 清空屏幕");
                        Console.WriteLine("  help - 显示帮助信息");
                        Console.WriteLine("  context - 显示当前对话ID");
                        Console.WriteLine("  switch - 切换对话");
                        continue;
                    }
                    
                    if (userInput == "context")
                    {
                        Console.WriteLine($"当前对话ID: {contextId}");
                        continue;
                    }
                    
                    if (userInput == "switch")
                    {
                        Console.Write("请输入新的对话ID: ");
                        var newContextId = Console.ReadLine();
                        if (!string.IsNullOrEmpty(newContextId))
                        {
                            contextId = newContextId;
                            Console.WriteLine($"切换到对话: {contextId}");
                        }
                        continue;
                    }
                    
                    if (string.IsNullOrEmpty(userInput))
                    {
                        continue;
                    }

                    var request = new { Message = userInput, ContextId = contextId, Stream = stream };
                    var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("/api/chat", content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ChatResponse>(json);
                        if (result?.Success == true)
                        {
                            Console.WriteLine($"AI: {result.Response}");
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
                var response = await _httpClient.GetAsync("/api/context");
                
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

        private static Command CreateHelpCommand()
        {
            var command = new Command("help", "显示帮助信息");

            command.SetAction((parseResult) =>
            {
                Console.WriteLine("EyuBot CLI工具帮助");
                Console.WriteLine("==================");
                Console.WriteLine();
                Console.WriteLine("命令列表:");
                Console.WriteLine("  chat      - 启动对话交互");
                Console.WriteLine("  config    - 配置引导工具");
                Console.WriteLine("  context   - 切换对话上下文");
                Console.WriteLine("  contexts  - 列出所有对话上下文");
                Console.WriteLine("  create    - 创建新对话");
                Console.WriteLine("  delete    - 删除对话");
                Console.WriteLine("  status    - 显示系统状态");
                Console.WriteLine("  help      - 显示帮助信息");
                Console.WriteLine("  version   - 显示版本信息");
                Console.WriteLine("  about     - 显示关于信息");
                Console.WriteLine();
                Console.WriteLine("使用示例:");
                Console.WriteLine("  dotnet run --project src/EyuBot.CLI chat \"Hello, EyuBot!\"");
                Console.WriteLine("  dotnet run --project src/EyuBot.CLI config");
                Console.WriteLine("  dotnet run --project src/EyuBot.CLI contexts");
                return 0;
            });

            return command;
        }

        private static Command CreateVersionCommand()
        {
            var command = new Command("version", "显示版本信息");

            command.SetAction((parseResult) =>
            {
                Console.WriteLine("EyuBot CLI工具");
                Console.WriteLine("版本: 1.0.0");
                Console.WriteLine("版权所有 © 2026 EyuBot Team");
                return 0;
            });

            return command;
        }

        private static Command CreateAboutCommand()
        {
            var command = new Command("about", "显示关于信息");

            command.SetAction((parseResult) =>
            {
                Console.WriteLine("关于 EyuBot");
                Console.WriteLine("============");
                Console.WriteLine();
                Console.WriteLine("EyuBot 是一个基于 C# 和 .NET 10 开发的 AI Agent 系统，");
                Console.WriteLine("支持与大模型对话、MCP集成、Skill系统和Subagent协作。");
                Console.WriteLine();
                Console.WriteLine("核心功能:");
                Console.WriteLine("  - 多模型提供商支持（OpenAI、Anthropic）");
                Console.WriteLine("  - 对话历史管理和持久化");
                Console.WriteLine("  - 功能完整的CLI工具");
                Console.WriteLine("  - 配置管理和引导工具");
                Console.WriteLine("  - 基于SQLite的存储系统");
                Console.WriteLine();
                Console.WriteLine("项目地址: https://github.com/eyubot/eyubot");
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
