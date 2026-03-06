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
    /// OpenAI/NVIDIA LLM provider
    /// </summary>
    public class OpenAiLlmProvider : LlmProviderBase
    {
        private readonly HttpClient _httpClient;
        private readonly LlmProviderConfig _config;

        /// <summary>
        /// Gets the provider name
        /// </summary>
        public override string ProviderName => _config.ProviderName;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiLlmProvider"/> class
        /// </summary>
        /// <param name="config">The LLM provider configuration</param>
        public OpenAiLlmProvider(LlmProviderConfig config)
        {
            _config = config;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Sends a message with conversation history to the LLM and gets a response
        /// </summary>
        /// <param name="messages">The conversation history</param>
        /// <returns>The LLM response</returns>
        public override async Task<string> GetResponseAsync(Message[] messages)
        {
            var requestBody = new
            {
                model = _config.Model,
                messages = messages.Select(m => new
                {
                    role = m.Role switch
                    {
                        MessageRole.System => "system",
                        MessageRole.User => "user",
                        MessageRole.Assistant => "assistant",
                        _ => "user"
                    },
                    content = m.Content
                }).ToArray(),
                temperature = 0.7
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            
            // NVIDIA API uses /chat/completions endpoint
            var endpoint = _config.ApiEndpoint.EndsWith("/chat/completions") ? 
                _config.ApiEndpoint : 
                $"{_config.ApiEndpoint}/chat/completions";
                
            var response = await _httpClient.PostAsync(endpoint, content);
            
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {ex.Message}. Response: {errorContent}");
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObj = JsonConvert.DeserializeObject<OpenAiResponse>(responseContent);
            
            return responseObj.choices[0].message.content;
        }

        /// <summary>
        /// Sends a message with conversation history to the LLM and gets a streaming response
        /// </summary>
        /// <param name="messages">The conversation history</param>
        /// <param name="onTokenReceived">Callback for each token received</param>
        /// <returns>The complete response</returns>
        public override async Task<string> GetStreamingResponseAsync(Message[] messages, System.Action<string> onTokenReceived)
        {
            var requestBody = new
            {
                model = _config.Model,
                messages = messages.Select(m => new
                {
                    role = m.Role switch
                    {
                        MessageRole.System => "system",
                        MessageRole.User => "user",
                        MessageRole.Assistant => "assistant",
                        _ => "user"
                    },
                    content = m.Content
                }).ToArray(),
                temperature = 0.7,
                stream = true
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            
            // NVIDIA API uses /chat/completions endpoint
            var endpoint = _config.ApiEndpoint.EndsWith("/chat/completions") ? 
                _config.ApiEndpoint : 
                $"{_config.ApiEndpoint}/chat/completions";
                
            var response = await _httpClient.PostAsync(endpoint, content, new System.Threading.CancellationToken());
            
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {ex.Message}. Response: {errorContent}");
            }
            
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
                        var chunk = JsonConvert.DeserializeObject<OpenAiStreamResponse>(data);
                        if (chunk.choices[0].delta?.content != null)
                        {
                            var token = chunk.choices[0].delta.content;
                            completeResponse.Append(token);
                            onTokenReceived(token);
                        }
                    }
                    catch { }
                }
            }
            
            return completeResponse.ToString();
        }

        private class OpenAiResponse
        {
            public Choice[] choices { get; set; }
        }

        private class Choice
        {
            public MessageResponse message { get; set; }
        }

        private class MessageResponse
        {
            public string content { get; set; }
        }

        private class OpenAiStreamResponse
        {
            public StreamChoice[] choices { get; set; }
        }

        private class StreamChoice
        {
            public Delta delta { get; set; }
        }

        private class Delta
        {
            public string content { get; set; }
        }
    }
}
