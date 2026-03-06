using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EyuBot.Core;
using EyuBot.Core.DependencyInjection;
using EyuBot.Core.LLM;
using EyuBot.Storage;
using EyuBot.CLI.Config;
using EyuBot.Abstractions;

namespace EyuBot.App
{
    /// <summary>
    /// Entry point for the EyuBot background service
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add EyuBot core services
            builder.Services.AddEyuBotCore(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                // Swagger is temporarily disabled due to compatibility issues with .NET 10
            }

            app.UseHttpsRedirection();

            // Add authentication middleware
            app.Use(async (context, next) =>
            {
                var configManager = new ConfigManager();
                var expectedToken = configManager.Configuration["Mcp:Token"];
                
                // Skip authentication if no token is configured
                if (string.IsNullOrEmpty(expectedToken))
                {
                    await next();
                    return;
                }
                
                var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                if (authHeader == null || !authHeader.StartsWith("Bearer "))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { Success = false, Error = "Unauthorized: No token provided" });
                    return;
                }
                
                var token = authHeader.Substring(7); // Remove "Bearer " prefix
                if (token != expectedToken)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { Success = false, Error = "Unauthorized: Invalid token" });
                    return;
                }
                
                await next();
            });

            // Define API endpoints
            app.MapPost("/api/chat", async (ChatRequest request) =>
            {
                try
                {
                    var configManager = new ConfigManager();
                    var storage = CreateStorage(configManager);
                    if (storage != null)
                    {
                        await storage.InitializeAsync();
                    }

                    var contextManager = new ContextManager(50, storage);
                    var history = await contextManager.GetOrCreateContextAsync(request.ContextId ?? "default");

                    // Get response from LLM (simulated for now)
                    await Task.Delay(1000);
                    var response = "This is a test response: " + request.Message;

                    // Save conversation
                    history.AddUserMessage(request.Message);
                    history.AddAssistantMessage(response);
                    await contextManager.SaveContextAsync(request.ContextId ?? "default");

                    return Results.Ok(new ChatResponse { Response = response, Success = true });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new ChatResponse { Success = false, Error = ex.Message });
                }
            });

            app.MapPost("/api/context/switch", async (ContextRequest request) =>
            {
                try
                {
                    var configManager = new ConfigManager();
                    var storage = CreateStorage(configManager);
                    if (storage != null)
                    {
                        await storage.InitializeAsync();
                    }

                    var contextManager = new ContextManager(50, storage);
                    var history = await contextManager.GetOrCreateContextAsync(request.ContextId);

                    return Results.Ok(new ContextResponse { ContextId = request.ContextId, MessageCount = history.Messages.Count, Success = true });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new ContextResponse { Success = false, Error = ex.Message });
                }
            });

            app.MapGet("/api/contexts", async () =>
            {
                try
                {
                    var configManager = new ConfigManager();
                    var storage = CreateStorage(configManager);
                    if (storage != null)
                    {
                        await storage.InitializeAsync();
                    }

                    var contextManager = new ContextManager(50, storage);
                    var conversations = await contextManager.ListConversationsAsync();

                    return Results.Ok(new ContextListResponse { ContextIds = conversations.ToArray(), Success = true });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new ContextListResponse { Success = false, Error = ex.Message });
                }
            });

            app.MapPost("/api/context/create", async () =>
            {
                try
                {
                    var conversationId = Guid.NewGuid().ToString();
                    return Results.Ok(new ContextResponse { ContextId = conversationId, MessageCount = 0, Success = true });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new ContextResponse { Success = false, Error = ex.Message });
                }
            });

            app.MapDelete("/api/context/{id}", async (string id) =>
            {
                try
                {
                    var configManager = new ConfigManager();
                    var storage = CreateStorage(configManager);
                    if (storage != null)
                    {
                        await storage.InitializeAsync();
                    }

                    var contextManager = new ContextManager(50, storage);
                    var result = await contextManager.RemoveContextAsync(id);

                    return Results.Ok(new DeleteResponse { Success = result });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new DeleteResponse { Success = false, Error = ex.Message });
                }
            });

            app.MapGet("/api/status", async () =>
            {
                try
                {
                    var configManager = new ConfigManager();
                    var storage = CreateStorage(configManager);
                    if (storage != null)
                    {
                        await storage.InitializeAsync();
                    }

                    var contextManager = new ContextManager(50, storage);
                    var conversations = await contextManager.ListConversationsAsync();

                    return Results.Ok(new StatusResponse
                    {
                        Status = "Running",
                        StorageType = configManager.Configuration["Storage:Type"] ?? "Sqlite",
                        ConversationCount = conversations.Count(),
                        Success = true
                    });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new StatusResponse { Success = false, Error = ex.Message });
                }
            });

            app.Run();
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
    }

    public class ChatResponse
    {
        public string Response { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }

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

    public class StatusResponse
    {
        public string Status { get; set; }
        public string StorageType { get; set; }
        public int ConversationCount { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}
