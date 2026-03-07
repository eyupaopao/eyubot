using EyuBot.CLI.Config;
using EyuBot.Core.DependencyInjection;
using IPCAS.Controllers.Options;
using System.Text;

namespace EyuBot.App
{
    /// <summary>
    /// Entry point for the EyuBot background service
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var builder = WebApplication.CreateBuilder(args);

            // Add EyuBot core services
            builder.Services.AddEyuBotCore(builder.Configuration);

            // Add MVC controllers
            builder.Services.AddControllers();

            // Add OpenAPI services
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var expectedToken = builder.Configuration["Mcp:Token"];
            var app = builder.Build();


            // app.UseHttpsRedirection(); // 禁用HTTPS重定向以方便本地开发

            // Add authentication middleware
            app.Use(async (context, next) =>
            {
                // Skip authentication for Swagger-related paths
                var path = context.Request.Path.Value;
                if (path.StartsWith("/swagger") || path.StartsWith("/swagger-ui"))
                {
                    await next();
                    return;
                }
                
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

            // Map controllers
            app.MapControllers();
            app.UseSwagger();
            app.UseSwaggerUI();

            app.Run();
        }
    }
}
