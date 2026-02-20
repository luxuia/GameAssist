using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameAssist.Models;

namespace GameAssist.Services;

public class OpenAIService
{
    private readonly HttpClient _httpClient;
    private string _apiKey = string.Empty;
    private string _endpoint = "https://api.openai.com/v1/chat/completions";
    private string _modelName = "gpt-4o";
    private int _maxTokens = 500;
    private double _temperature = 0.7;
    private ApiProvider _provider = ApiProvider.OpenAI;

    public OpenAIService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public void Configure(ApiProvider provider, string apiKey, string? endpoint = null, string? modelName = null, int? maxTokens = null, double? temperature = null)
    {
        _provider = provider;
        _apiKey = apiKey;
        if (!string.IsNullOrEmpty(endpoint)) _endpoint = endpoint;
        if (!string.IsNullOrEmpty(modelName)) _modelName = modelName;
        if (maxTokens.HasValue) _maxTokens = maxTokens.Value;
        if (temperature.HasValue) _temperature = temperature.Value;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<string?> AnalyzeImageAsync(byte[] imageBytes, string prompt, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("API key is not configured.");

        var base64Image = Convert.ToBase64String(imageBytes);

        // Prepare request based on provider
        HttpContent content;
        bool isZhipuAI = _provider == ApiProvider.ZhipuAI;
        bool isDoubao = _provider == ApiProvider.Doubao;

        if (isZhipuAI || isDoubao)
        {
            // GLM-4.6V and Doubao use JSON format with specific structure
            var request = CreateRequest(prompt, base64Image);
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        else
        {
            // OpenAI format
            var request = CreateRequest(prompt, base64Image);
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        _httpClient.DefaultRequestHeaders.Clear();

        // Set authorization header based on provider
        if (isZhipuAI)
        {
            var token = GenerateZhipuToken(_apiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }
        else if (isDoubao)
        {
            // Doubao uses Bearer token with API Key
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        try
        {
            var response = await _httpClient.PostAsync(_endpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"API request failed: {response.StatusCode} - {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            // Use different response model for ZhipuAI and Doubao
            if (isZhipuAI)
            {
                var result = JsonSerializer.Deserialize<ZhipuChatCompletionResponse>(responseJson);
                return result?.Choices?.FirstOrDefault()?.Message?.Content;
            }
            else if (isDoubao)
            {
                // Doubao uses OpenAI-compatible response format
                var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson);
                return result?.Choices?.FirstOrDefault()?.Message?.Content;
            }
            else
            {
                var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson);
                return result?.Choices?.FirstOrDefault()?.Message?.Content;
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to analyze image: {ex.Message}", ex);
        }
    }

    private ChatCompletionRequest CreateRequest(string prompt, string base64Image)
    {
        // Add system instruction to respond in Chinese
        var systemPrompt = "你必须用中文回复所有内容。无论用户用什么语言提问，都要用中文回答。";

        // Zhipu AI GLM-4.6.6V uses different API format
        if (_provider == ApiProvider.ZhipuAI)
        {
            // GLM-4.6V uses a simpler message format for images
            return new ChatCompletionRequest
            {
                Model = _modelName,
                Messages = new object[]
                {
                    new SystemMessage { Role = "system", Content = systemPrompt },
                    new Message
                    {
                        Role = "user",
                        Content = new object[]
                        {
                            new { type = "text", text = prompt },
                            new
                            {
                                type = "image_url",
                                image_url = new
                                {
                                    url = $"data:image/jpeg;base64,{base64Image}",
                                    detail = "high"  // Add detail parameter for better image analysis
                                }
                            }
                        }
                    }
                }.Cast<object>().ToArray(),
                MaxTokens = _maxTokens,
                Temperature = _temperature,
                Stream = false
            };
        }
        else if (_provider == ApiProvider.Doubao)
        {
            // Doubao (Volcengine) uses OpenAI-compatible format
            return new ChatCompletionRequest
            {
                Model = _modelName,
                Messages = new object[]
                {
                    new SystemMessage { Role = "system", Content = systemPrompt },
                    new Message
                    {
                        Role = "user",
                        Content = new object[]
                        {
                            new { type = "text", text = prompt },
                            new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{base64Image}" } }
                        }
                    }
                }.Cast<object>().ToArray(),
                MaxTokens = _maxTokens,
                Temperature = _temperature
            };
        }
        else
        {
            // OpenAI format - GPT-4o Vision requires data URL prefix
            return new ChatCompletionRequest
            {
                Model = _modelName,
                Messages = new object[]
                {
                    new SystemMessage { Role = "system", Content = systemPrompt },
                    new Message
                    {
                        Role = "user",
                        Content = new object[]
                        {
                            new { type = "text", text = prompt },
                            new { type = "image_url", image_url = new { url = $"data:image/png;base64,{base64Image}" } }
                        }
                    }
                }.Cast<object>().ToArray(),
                MaxTokens = _maxTokens,
                Temperature = _temperature
            };
        }
    }

    // Generate JWT token for Zhipu AI authentication
    private string GenerateZhipuToken(string apiKey)
    {
        try
        {
            var parts = apiKey.Split('.');
            if (parts.Length != 2)
                throw new ArgumentException("Invalid Zhipu AI API key format. Expected: id.secret");

            var id = parts[0];
            var secret = parts[1];

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000;
            var exp = timestamp + 3600; // Token expires in 1 hour

            var header = new { alg = "HS256", sign_type = "SIGN" };
            var payload = new { api_key = id, exp = exp, timestamp = timestamp };

            var headerJson = JsonSerializer.Serialize(header);
            var payloadJson = JsonSerializer.Serialize(payload);

            var headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
            var payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));

            var message = $"{headerBase64}.{payloadBase64}";
            var keyBytes = Encoding.UTF8.GetBytes(secret);

            using var hmac = new HMACSHA256(keyBytes);
            var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            var signatureBase64 = Base64UrlEncode(signature);

            return $"{message}.{signatureBase64}";
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to generate Zhipu AI token: {ex.Message}", ex);
        }
    }

    private string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    // Request and response models for Chat Completion API
    private class ChatCompletionRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public object[] Messages { get; set; } = Array.Empty<object>();

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
    }

    private class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public object Content { get; set; } = Array.Empty<object>();
    }

    private class SystemMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private class ChatCompletionResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("object")]
        public string? ObjectType { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("choices")]
        public Choice[]? Choices { get; set; }

        [JsonPropertyName("usage")]
        public Usage? Usage { get; set; }
    }

    private class Choice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public MessageContent? Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    private class MessageContent
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    // Zhipu AI GLM-4.6V response model
    private class ZhipuChatCompletionResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("object")]
        public string? ObjectType { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("choices")]
        public ZhipuChoice[]? Choices { get; set; }

        [JsonPropertyName("usage")]
        public Usage? Usage { get; set; }
    }

    private class ZhipuChoice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public ZhipuMessageContent? Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    private class ZhipuMessageContent
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
