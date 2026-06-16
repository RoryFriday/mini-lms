using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibraryApi.Services.Ai;

public class OpenAiProvider : IAiProvider
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly string? _apiKey;
    private readonly ILogger<OpenAiProvider> _logger;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

    public OpenAiProvider(IConfiguration config, ILogger<OpenAiProvider> logger)
    {
        _logger = logger;
        _apiKey = config["Ai:OpenAi:ApiKey"];
        _model = config["Ai:OpenAi:Model"] ?? "gpt-4o";

        _http = new HttpClient
        {
            BaseAddress = new Uri("https://api.openai.com/v1/")
        };

        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }
    }

    public async Task<string> ChatAsync(string systemPrompt, string userMessage)
    {
        return await ChatAsync(systemPrompt, new List<ChatMessage>
        {
            new() { Role = "user", Content = userMessage }
        });
    }

    public async Task<string> ChatAsync(string systemPrompt, List<ChatMessage> messages)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("OpenAI API key is not configured.");

        var allMessages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };
        allMessages.AddRange(messages.Select(m => (object)new { role = m.Role, content = m.Content }));

        var payload = new
        {
            model = _model,
            messages = allMessages,
            temperature = 0.3,
            max_tokens = 2048
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Sending request to OpenAI ({Model})", _model);

        var response = await _http.PostAsync("chat/completions", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OpenAI API error: {StatusCode} - {Body}", response.StatusCode, responseBody);
            throw new HttpRequestException($"OpenAI API error: {response.StatusCode}");
        }

        var doc = JsonDocument.Parse(responseBody);
        var result = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return result ?? string.Empty;
    }
}
