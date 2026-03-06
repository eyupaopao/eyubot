# LLM 模块技术文档

## 概述

LLM（大语言模型）模块是eyubot系统的核心组件，负责与各种大语言模型进行交互，提供对话功能。该模块支持多种LLM提供商，包括OpenAI和Anthropic，并提供了对话历史管理、上下文管理等功能。

## 架构设计

### 核心组件

1. **ILlmProvider** - LLM提供商接口，定义了与LLM交互的方法
2. **LlmProviderBase** - LLM提供商基类，实现了通用功能
3. **OpenAiLlmProvider** - OpenAI API集成
4. **AnthropicLlmProvider** - Anthropic API集成
5. **ConversationHistory** - 对话历史管理
6. **MessageFormatter** - 消息格式化工具
7. **ContextManager** - 上下文管理
8. **Message** - 消息模型

### 类图

```mermaid
classDiagram
    class ILlmProvider {
        +ProviderName: string
        +GetResponseAsync(message: string): Task<string>
        +GetResponseAsync(messages: Message[]): Task<string>
        +GetStreamingResponseAsync(message: string, onTokenReceived: Action<string>): Task<string>
        +GetStreamingResponseAsync(messages: Message[], onTokenReceived: Action<string>): Task<string>
    }

    class LlmProviderBase {
        +ProviderName: string
        +GetResponseAsync(message: string): Task<string>
        +GetResponseAsync(messages: Message[]): Task<string>
        +GetStreamingResponseAsync(message: string, onTokenReceived: Action<string>): Task<string>
        +GetStreamingResponseAsync(messages: Message[], onTokenReceived: Action<string>): Task<string>
    }

    class OpenAiLlmProvider {
        +ProviderName: string
        +GetResponseAsync(messages: Message[]): Task<string>
        +GetStreamingResponseAsync(messages: Message[], onTokenReceived: Action<string>): Task<string>
    }

    class AnthropicLlmProvider {
        +ProviderName: string
        +GetResponseAsync(messages: Message[]): Task<string>
        +GetStreamingResponseAsync(messages: Message[], onTokenReceived: Action<string>): Task<string>
    }

    class ConversationHistory {
        +Messages: IReadOnlyList<Message>
        +AddMessage(message: Message): void
        +AddUserMessage(content: string): void
        +AddAssistantMessage(content: string): void
        +AddSystemMessage(content: string): void
        +Clear(): void
        +ToArray(): Message[]
    }

    class MessageFormatter {
        +FormatSystemMessage(content: string): Message
        +FormatUserMessage(content: string): Message
        +FormatAssistantMessage(content: string): Message
        +FormatConversation(userMessage: string, systemPrompt: string): Message[]
        +FormatConversation(userMessage: string, history: ConversationHistory, systemPrompt: string): Message[]
    }

    class ContextManager {
        +GetOrCreateContext(contextId: string): ConversationHistory
        +RemoveContext(contextId: string): bool
        +ClearAllContexts(): void
        +ContextCount: int
    }

    class Message {
        +Role: MessageRole
        +Content: string
        +Message(role: MessageRole, content: string)
    }

    enum MessageRole {
        System
        User
        Assistant
    }

    ILlmProvider <|-- LlmProviderBase
    LlmProviderBase <|-- OpenAiLlmProvider
    LlmProviderBase <|-- AnthropicLlmProvider
    ContextManager *-- ConversationHistory
    ConversationHistory *-- Message
    MessageFormatter --> Message
```

## 功能特性

### 1. 多LLM提供商支持

- **OpenAI** - 支持GPT系列模型
- **Anthropic** - 支持Claude系列模型
- 可扩展架构，易于添加新的LLM提供商

### 2. 对话管理

- **对话历史** - 自动管理对话历史，支持多轮对话
- **上下文管理** - 支持多用户、多会话的上下文隔离
- **消息格式化** - 提供统一的消息格式化工具

### 3. 流式响应

- 支持流式响应，实时显示LLM生成的内容
- 提供token级别的回调，可用于实现打字效果

### 4. 配置管理

- 通过`LlmProviderConfig`配置LLM提供商信息
- 支持API密钥管理、模型选择等

## 使用方法

### 基本使用

```csharp
// 1. 创建LLM提供商实例
var config = new LlmProviderConfig {
    ProviderName = "OpenAI",
    ApiKey = "your-api-key",
    ApiEndpoint = "https://api.openai.com/v1/chat/completions",
    Model = "gpt-4"
};

var llmProvider = new OpenAiLlmProvider(config);

// 2. 发送消息并获取响应
var response = await llmProvider.GetResponseAsync("Hello, how are you?");
Console.WriteLine(response);

// 3. 使用流式响应
var streamingResponse = new StringBuilder();
await llmProvider.GetStreamingResponseAsync("Tell me a story", token => {
    streamingResponse.Append(token);
    Console.Write(token); // 实时显示
});
```

### 对话历史管理

```csharp
// 1. 创建对话历史
var history = new ConversationHistory();

// 2. 添加消息
history.AddUserMessage("Hello");
history.AddAssistantMessage("Hi, how can I help you?");
history.AddUserMessage("What's the weather like today?");

// 3. 发送带历史的消息
var messages = history.ToArray();
var response = await llmProvider.GetResponseAsync(messages);

// 4. 添加助手响应到历史
history.AddAssistantMessage(response);
```

### 上下文管理

```csharp
// 1. 创建上下文管理器
var contextManager = new ContextManager();

// 2. 为不同用户获取或创建上下文
var user1Context = contextManager.GetOrCreateContext("user1");
var user2Context = contextManager.GetOrCreateContext("user2");

// 3. 管理不同用户的对话
user1Context.AddUserMessage("Hello");
user2Context.AddUserMessage("Hi there");

// 4. 清理上下文
contextManager.RemoveContext("user1");
```

### 消息格式化

```csharp
// 1. 格式化单条消息
var userMessage = MessageFormatter.FormatUserMessage("Hello");
var assistantMessage = MessageFormatter.FormatAssistantMessage("Hi");
var systemMessage = MessageFormatter.FormatSystemMessage("You are a helpful assistant");

// 2. 格式化对话
var messages = MessageFormatter.FormatConversation(
    "What's the capital of France?",
    "You are a geography expert"
);

// 3. 带历史的格式化
var history = new ConversationHistory();
history.AddUserMessage("Hello");
history.AddAssistantMessage("Hi");

var messagesWithHistory = MessageFormatter.FormatConversation(
    "What's the capital of France?",
    history,
    "You are a geography expert"
);
```

## 依赖项

- **Newtonsoft.Json** - 用于JSON序列化/反序列化
- **System.Net.Http** - 用于HTTP请求
- **Microsoft.Extensions.DependencyInjection** - 用于依赖注入

## 配置项

| 配置项 | 类型 | 说明 | 默认值 |
|--------|------|------|--------|
| ProviderName | string | LLM提供商名称 | "OpenAI" |
| ApiEndpoint | string | API端点URL | "https://api.openai.com/v1/chat/completions" |
| ApiKey | string | API密钥 | "" |
| Model | string | 模型名称 | "gpt-4" |

## 扩展指南

### 添加新的LLM提供商

1. 创建一个继承自`LlmProviderBase`的类
2. 实现`ProviderName`属性
3. 实现`GetResponseAsync`和`GetStreamingResponseAsync`方法
4. 在依赖注入中注册新的提供商

```csharp
public class CustomLlmProvider : LlmProviderBase
{
    public override string ProviderName => "Custom";
    
    public override Task<string> GetResponseAsync(Message[] messages)
    {
        // 实现自定义LLM调用逻辑
    }
    
    public override Task<string> GetStreamingResponseAsync(Message[] messages, Action<string> onTokenReceived)
    {
        // 实现自定义流式响应逻辑
    }
}

// 在依赖注入中注册
services.AddTransient<ILlmProvider, CustomLlmProvider>();
```

### 自定义消息格式化

可以扩展`MessageFormatter`类，添加自定义的消息格式化方法，以适应特定的LLM提供商需求。

## 测试

LLM模块包含完整的单元测试，测试覆盖了以下功能：

- **Message** - 消息模型测试
- **ConversationHistory** - 对话历史管理测试
- **ContextManager** - 上下文管理测试
- **MessageFormatter** - 消息格式化测试

测试使用xUnit框架，可通过以下命令运行：

```bash
dotnet test
```

## 性能优化

1. **缓存** - 可以添加缓存机制，缓存常见查询的响应
2. **批量请求** - 对于多个独立查询，可以考虑批量处理
3. **连接池** - 使用HTTP连接池，减少连接建立的开销
4. **异步处理** - 充分利用异步编程，提高并发处理能力

## 安全注意事项

1. **API密钥管理** - 不要在代码中硬编码API密钥，使用配置文件或环境变量
2. **输入验证** - 对用户输入进行验证，防止注入攻击
3. **速率限制** - 遵守LLM提供商的速率限制，避免被封禁
4. **数据隐私** - 确保敏感数据不会被发送到LLM

## 未来计划

1. **更多LLM提供商** - 支持更多的LLM提供商，如Google Gemini、Meta Llama等
2. **模型微调** - 支持模型微调功能
3. **本地模型** - 支持本地部署的LLM模型
4. **高级对话管理** - 实现更复杂的对话管理策略，如主题检测、情感分析等
