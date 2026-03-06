using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EyuBot.Core.Configuration;
using EyuBot.Core.LLM;

namespace EyuBot.Core.DependencyInjection
{
    /// <summary>
    /// Extension methods for configuring EyuBot services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds EyuBot core services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddEyuBotCore(this IServiceCollection services, IConfiguration configuration)
        {
            // Register configuration
            var appConfig = new AppConfig();
            configuration.Bind(appConfig);
            services.AddSingleton(appConfig);
            services.AddSingleton(appConfig.Agent);
            services.AddSingleton(appConfig.LlmProvider);
            services.AddSingleton(appConfig.Mcp);

            // Register configuration sections separately for easier access
            services.Configure<AgentConfig>(options =>
            {
                configuration.GetSection(nameof(AppConfig.Agent)).Bind(options);
            });
            services.Configure<LlmProviderConfig>(options =>
            {
                configuration.GetSection(nameof(AppConfig.LlmProvider)).Bind(options);
            });
            services.Configure<McpConfig>(options =>
            {
                configuration.GetSection(nameof(AppConfig.Mcp)).Bind(options);
            });

            // Register LLM services
            services.AddTransient<ILlmProvider, OpenAiLlmProvider>();
            services.AddSingleton<ContextManager>();
            services.AddTransient<ConversationHistory>();

            return services;
        }
    }
}
