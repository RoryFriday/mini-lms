namespace LibraryApi.Services.Ai;

public class ChatMessage
{
    public string Role { get; set; } = string.Empty;  // "system", "user", "assistant"
    public string Content { get; set; } = string.Empty;
}

public interface IAiProvider
{
    Task<string> ChatAsync(string systemPrompt, string userMessage);
    Task<string> ChatAsync(string systemPrompt, List<ChatMessage> messages);
    bool IsConfigured { get; }
}
