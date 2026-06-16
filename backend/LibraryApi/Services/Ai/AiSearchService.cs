using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Services.Ai.Prompts;

namespace LibraryApi.Services.Ai;

public record AiSearchResult(
    IEnumerable<AiBookResult> Items,
    int TotalCount
);

public record AiBookResult(
    BookDto Book,
    double Score,
    string Reason
);

public interface IAiSearchService
{
    bool IsAvailable { get; }
    Task<AiSearchResult> SearchAsync(string query);
}

public class AiSearchService : IAiSearchService
{
    private readonly IAiProvider _aiProvider;
    private readonly LibraryDbContext _db;
    private readonly ILogger<AiSearchService> _logger;

    public bool IsAvailable => _aiProvider.IsConfigured;

    public AiSearchService(IAiProvider aiProvider, LibraryDbContext db, ILogger<AiSearchService> logger)
    {
        _aiProvider = aiProvider;
        _db = db;
        _logger = logger;
    }

    public async Task<AiSearchResult> SearchAsync(string query)
    {
        if (!_aiProvider.IsConfigured)
            throw new InvalidOperationException("AI provider is not configured.");

        // Fetch all books to build the catalog context
        var allBooks = await _db.Books
            .Select(b => new
            {
                b.Id,
                b.Title,
                b.Author,
                b.Genre,
                b.Description,
                b.PublicationYear,
                b.ISBN
            })
            .ToListAsync();

        var catalogJson = JsonSerializer.Serialize(allBooks, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var userMessage = BookSearchPrompts.BuildUserMessage(query, catalogJson);

        _logger.LogInformation("AI search for query: \"{Query}\" across {Count} books", query, allBooks.Count);

        var aiResponse = await _aiProvider.ChatAsync(BookSearchPrompts.SystemPrompt, userMessage);

        // Parse the AI response — strip markdown code fences if present
        var cleaned = aiResponse.Trim();
        if (cleaned.StartsWith("```"))
        {
            var firstNewline = cleaned.IndexOf('\n');
            if (firstNewline >= 0)
                cleaned = cleaned[(firstNewline + 1)..];
            if (cleaned.EndsWith("```"))
                cleaned = cleaned[..^3];
            cleaned = cleaned.Trim();
        }

        List<AiMatchResult>? matches;
        try
        {
            matches = JsonSerializer.Deserialize<List<AiMatchResult>>(cleaned, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse AI response: {Response}", cleaned);
            return new AiSearchResult(Enumerable.Empty<AiBookResult>(), 0);
        }

        if (matches == null || matches.Count == 0)
            return new AiSearchResult(Enumerable.Empty<AiBookResult>(), 0);

        // Fetch the matched books with full details
        var matchedIds = matches.Select(m => m.Id).ToHashSet();
        var bookEntities = await _db.Books
            .Where(b => matchedIds.Contains(b.Id))
            .ToListAsync();

        var bookLookup = bookEntities.ToDictionary(b => b.Id);

        var results = matches
            .Where(m => bookLookup.ContainsKey(m.Id))
            .OrderByDescending(m => m.Score)
            .Select(m =>
            {
                var b = bookLookup[m.Id];
                var dto = new BookDto(b.Id, b.Title, b.Author, b.ISBN, b.Genre, b.Description,
                    b.PublicationYear, b.Publisher, b.TotalCopies, b.AvailableCopies,
                    b.CreatedAt, b.UpdatedAt);
                return new AiBookResult(dto, m.Score, m.Reason);
            })
            .ToList();

        return new AiSearchResult(results, results.Count);
    }

    private class AiMatchResult
    {
        public int Id { get; set; }
        public double Score { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
