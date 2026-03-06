using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using EyuBot.Core.Configuration;

namespace EyuBot.Core.LLM
{
    /// <summary>
    /// Anthropic LLM provider
    /// </summary>
    public class AnthropicLlmProvider : LlmProviderBase
    {
        private readonly HttpClient _httpClient;
        private readonly LlmProviderConfig _config;

        /// <summary>
        /// Gets the provider name
        /// </summary>
        public override string ProviderName => "Anthropic";

        /// <summary>
        /// Initializes a new instance of the <see cref="AnthropicLlmProvider"/> class
        /// </summary>
        /// <param name="config">The LLM provider configuration</param>
        public AnthropicLlmProvider(LlmProviderConfig config)
        {
            _config = config;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        }

        /// <summary>
        /// Sends a message with conversation history to the LLM and gets a response
        /// </summary>
        /// <param name="messages">The conversation history</param>
        /// <returns>The LLM response</returns>
        public override async Task<string> GetResponseAsync(Message[] messages)
        {
            // Anthropic API requires system message to be in the system parameter, not as a message
            var systemMessage = messages.FirstOrDefault(m => m.Role == MessageRole.System)?.Content;
            var userMessages = messages.Where(m => m.Role != MessageRole.System).Select(m => new
            {
                role = m.Role switch
                {
                    MessageRole.User => "user",
                    MessageRole.Assistant => "assistant",
                    _ => "user"
                },
                content = m.Content
            }).ToArray();

            var requestBody = new
            {
                model = _config.Model,
                messages = userMessages,
                system = systemMessage,
                temperature = 0.7
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_config.ApiEndpoint, content);
            
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObj = JsonConvert.DeserializeObject<AnthropicResponse>(responseContent);
            
            return responseObj.content[0].text;
        }

        /// <summary>
        /// Sends a message with conversation history to the LLM and gets a streaming response
        /// </summary>
        /// <param name="messages">The conversation history</param>
        /// <param name="onTokenReceived">Callback for each token received</param>
        /// <returns>The complete response</returns>
        public override async Task<string> GetStreamingResponseAsync(Message[] messages, System.Action<string> onTokenReceived)
        {
            // Anthropic API requires system message to be in the system parameter, not as a message
            var systemMessage = messages.FirstOrDefault(m => m.Role == MessageRole.System)?.Content;
            var userMessages = messages.Where(m => m.Role != MessageRole.System).Select(m => new
            {
                role = m.Role switch
                {
                    MessageRole.User => "user",
                    MessageRole.Assistant => "assistant",
                    _ => "user"
                },
                content = m.Content
            }).ToArray();

            var requestBody = new
            {
                model = _config.Model,
                messages = userMessages,
                system = systemMessage,
                temperature = 0.7,
                stream = true
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_config.ApiEndpoint, content, new System.Threading.CancellationToken());
            
            response.EnsureSuccessStatusCode();
            
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new System.IO.StreamReader(stream);
            
            var completeResponse = new StringBuilder();
            
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("data: "))
                {
                    var data = line.Substring(6);
                    if (data == "[DONE]") break;
                    
                    try
                    {
                        var chunk = JsonConvert.DeserializeObject<AnthropicStreamResponse>(data);
                        if (chunk.type == "content_block_delta" && chunk.delta?.text != null)
                        {
                            var token = chunk.delta.text;
                            completeResponse.Append(token);
                            onTokenReceived(token);
                        }
                    }
                    catch { }
                }
            }
            
            return completeResponse.ToString();
        }

        private class AnthropicResponse
        {
            public Content[] content { get; set; }
        }

        private class Content
        {
            public string text { get; set; }
        }

        private class AnthropicStreamResponse
        {
            public string type { get; set; }
            public Delta delta { get; set; }
        }

        private class Delta
        {
            public string text { get; set; }
        }
    }
}
