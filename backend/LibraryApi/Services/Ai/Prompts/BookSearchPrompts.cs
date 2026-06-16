namespace LibraryApi.Services.Ai.Prompts;

public static class BookSearchPrompts
{
    public const string SystemPrompt = @"You are a library search assistant. Given a natural language query from a patron and a catalog of books, determine which books best match the patron's intent.

You MUST respond with ONLY a valid JSON array of objects, with no additional text, markdown, or explanation. Each object has:
- ""id"": the book's integer ID
- ""score"": a relevance score from 0.0 to 1.0 (1.0 = perfect match)
- ""reason"": a brief one-sentence explanation of why this book matches

Only include books with a score of 0.3 or higher. Order by score descending.

If no books match the query, respond with an empty JSON array: []

Example response format:
[{""id"":3,""score"":0.95,""reason"":""Directly addresses the theme of government surveillance and dystopia.""},{""id"":1,""score"":0.4,""reason"":""Explores societal themes tangentially related to the query.""}]";

    public static string BuildUserMessage(string query, string catalogJson)
    {
        return $@"Patron's search query: ""{query}""

Library catalog:
{catalogJson}";
    }
}
