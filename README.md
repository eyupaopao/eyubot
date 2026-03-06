# eyubot - AI Agent System

eyubot是一个基于C#开发的AI Agent系统，支持与大模型对话、MCP集成、Skill系统和Subagent协作。

## 架构设计

### 核心组件
1. **主Agent框架** - 负责整体控制和协调
2. **MCP集成模块** - Model Context Protocol集成
3. **Skill管理系统** - 插件化技能管理
4. **Subagent协作系统** - 多智能体协作机制
5. **大模型接口层** - 与LLM通信
6. **多IM平台适配器** - 支持多种即时通讯平台
7. **后台服务** - 提供API接口，处理核心业务逻辑
8. **CLI工具** - 命令行界面，与后台服务交互

### 系统架构
- **EyuBot.App** - 后台服务，基于ASP.NET Core，提供RESTful API
- **EyuBot.CLI** - 命令行工具，与后台服务通信
- **EyuBot.Core** - 核心框架，包含Agent、LLM接口等
- **EyuBot.Storage** - 存储系统，基于SQLite
- **EyuBot.Abstractions** - 抽象接口
- **EyuBot.IMAdapter** - IM平台适配器
- **EyuBot.MCP** - MCP协议集成
- **EyuBot.Skills** - 技能系统
- **EyuBot.Subagents** - 子代理系统

## 快速开始

### 运行后台服务
```bash
dotnet run --project src\EyuBot.App
```

### 使用CLI工具
```bash
# 查看系统状态
dotnet run --project src\EyuBot.CLI status

# 发送消息
dotnet run --project src\EyuBot.CLI chat "Hello, EyuBot!"

# 列出所有对话
dotnet run --project src\EyuBot.CLI contexts

# 创建新对话
dotnet run --project src\EyuBot.CLI create

# 删除对话
dotnet run --project src\EyuBot.CLI delete <conversation-id>

# 切换对话
dotnet run --project src\EyuBot.CLI context <conversation-id>

# 配置系统
dotnet run --project src\EyuBot.CLI config
```

## 实施计划

### 阶段1：项目基础架构搭建
- [x] 创建C#项目结构
- [x] 设置基本的Agent类框架
- [x] 配置依赖注入容器
- [x] 实现基础配置管理
- [x] 初始化Git仓库
- [x] 分离后台服务和CLI工具

### 阶段2：IM平台适配器开发
- [ ] 设计统一消息接口
- [ ] 实现配置管理系统（API Key管理）
- [ ] 开发各平台适配器基类
- [ ] 实现至少2个主流平台适配器（如Discord、Telegram）

### 阶段3：MCP集成
- [ ] 研究MCP协议规范
- [ ] 实现MCP客户端/服务器接口
- [ ] 集成MCP资源管理
- [ ] 测试MCP连接功能

### 阶段4：大模型对话功能
- [x] 实现大模型API接口抽象
- [x] 集成主流LLM提供商（如OpenAI、Anthropic等）
- [x] 实现对话历史管理
- [ ] 实现流式响应处理

### 阶段5：Skill系统
- [ ] 设计Skill插件架构
- [ ] 实现Skill注册和发现机制
- [ ] 开发基础Skill示例
- [ ] 实现Skill执行调度器

### 阶段6：Subagent协作系统
- [ ] 设计Subagent通信协议
- [ ] 实现任务分解和分配机制
- [ ] 实现Subagent生命周期管理
- [ ] 实现结果聚合机制

### 阶段7：集成测试与优化
- [ ] 进行端到端功能测试
- [ ] 性能优化
- [ ] 错误处理完善
- [ ] 文档编写