using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LibraryApi.Services.Ai;

public class AnthropicProvider : IAiProvider
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly string? _apiKey;
    private readonly ILogger<AnthropicProvider> _logger;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

    public AnthropicProvider(IConfiguration config, ILogger<AnthropicProvider> logger)
    {
        _logger = logger;
        _apiKey = config["Ai:Anthropic:ApiKey"];
        _model = config["Ai:Anthropic:Model"] ?? "claude-sonnet-4-20250514";

        _http = new HttpClient
        {
            BaseAddress = new Uri("https://api.anthropic.com/")
        };

        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            _http.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        }
        _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
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
            throw new InvalidOperationException("Anthropic API key is not configured.");

        var payload = new
        {
            model = _model,
            max_tokens = 2048,
            system = systemPrompt,
            messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray()
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Sending request to Anthropic ({Model})", _model);

        var response = await _http.PostAsync("v1/messages", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Anthropic API error: {StatusCode} - {Body}", response.StatusCode, responseBody);
            throw new HttpRequestException($"Anthropic API error: {response.StatusCode}");
        }

        var doc = JsonDocument.Parse(responseBody);
        var result = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString();

        return result ?? string.Empty;
    }
}
