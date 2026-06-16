# AI Features for the Library Management System

## What
Implement creative AI-powered features that enhance the library experience for patrons and librarians, using a provider-agnostic approach that supports both **Anthropic (Claude)** and **OpenAI (GPT)**.

---

## Feature Ideas

### 1. 📚 AI Book Recommendations
**Description:** Personalized book recommendations based on a patron's checkout history, reading preferences, and the library's catalog.

**How it works:**
- Collect the user's past checkouts (titles, authors, genres)
- Send a prompt to the AI with the user's history and the library catalog
- Return a ranked list of recommended books with brief explanations

**User Experience:** A "Recommended For You" section on the Books page, or a dedicated "Get Recommendations" button.

---

### 2. 🔍 Natural Language Book Search
**Description:** Allow patrons to search in natural language instead of exact keyword matches. E.g., *"a book about a dystopian society with surveillance"* → returns *1984*.

**How it works:**
- Accept a natural language query from the user
- Send the query + book catalog metadata (titles, descriptions, genres) to the AI
- AI returns the most relevant book IDs with relevance scores
- Display matched books sorted by relevance

**User Experience:** A toggle on the search bar: "Smart Search (AI)" vs. standard keyword search.

---

### 3. 📝 AI-Generated Book Summaries
**Description:** Auto-generate engaging book summaries/descriptions when a librarian adds a new book with only title + author.

**How it works:**
- When creating a book, if the description field is empty, offer "Generate with AI"
- Send title + author to the AI, ask for a 2-3 sentence library-appropriate description
- Auto-fill the description field (librarian can edit before saving)

**User Experience:** A ✨ button next to the Description field in the Add/Edit Book modal.

---

### 4. 💬 Library Chatbot Assistant
**Description:** A conversational chatbot that can answer patron questions about the library's catalog, policies, and help with finding books.

**How it works:**
- Floating chat widget in the frontend
- System prompt includes library context (catalog data, checkout policies, hours)
- Maintains conversation history for multi-turn dialogues
- Can surface direct links to books in responses

**User Experience:** A chat bubble in the bottom-right corner, available to all logged-in users.

---

### 5. 📊 Intelligent Overdue Notifications
**Description:** AI-crafted personalized reminder emails for overdue books that are friendly and contextual.

**How it works:**
- Scheduled job checks for overdue checkouts
- AI generates personalized reminder messages based on user name, book title, days overdue
- Tone adjusts based on how overdue (gentle reminder → firm notice)
- Sent via email (SES) or displayed as in-app notification

---

## Provider-Agnostic Architecture

### Design Pattern: Strategy/Adapter Pattern

```
┌──────────────────────┐
│   IAiProvider        │  ← Interface
│   + ChatAsync()      │
│   + CompleteAsync()   │
└──────┬───────┬───────┘
       │       │
┌──────▼──┐ ┌──▼────────┐
│ OpenAI  │ │ Anthropic  │  ← Concrete implementations
│ Provider│ │ Provider   │
└─────────┘ └────────────┘
```

### Backend Implementation

```csharp
// 1. Define the provider interface
public interface IAiProvider
{
    Task<string> ChatAsync(string systemPrompt, string userMessage);
    Task<string> ChatAsync(string systemPrompt, List<ChatMessage> messages);
}

// 2. OpenAI implementation
public class OpenAiProvider : IAiProvider
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;

    public OpenAiProvider(IConfiguration config)
    {
        _apiKey = config["Ai:OpenAi:ApiKey"];
        _model = config["Ai:OpenAi:Model"] ?? "gpt-4o";
        _http = new HttpClient { BaseAddress = new Uri("https://api.openai.com/v1/") };
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> ChatAsync(string systemPrompt, string userMessage)
    {
        // Call OpenAI Chat Completions API
        var payload = new { model = _model, messages = new[] {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = userMessage }
        }};
        // POST to /chat/completions, parse response
    }
}

// 3. Anthropic implementation
public class AnthropicProvider : IAiProvider
{
    // Similar structure, using Anthropic Messages API
    // POST to https://api.anthropic.com/v1/messages
}

// 4. Registration in Program.cs (based on configuration)
var aiProvider = builder.Configuration["Ai:Provider"]; // "OpenAi" or "Anthropic"
if (aiProvider == "Anthropic")
    builder.Services.AddSingleton<IAiProvider, AnthropicProvider>();
else
    builder.Services.AddSingleton<IAiProvider, OpenAiProvider>();
```

### Configuration

```json
{
  "Ai": {
    "Provider": "OpenAi",
    "OpenAi": {
      "ApiKey": "sk-...",
      "Model": "gpt-4o"
    },
    "Anthropic": {
      "ApiKey": "sk-ant-...",
      "Model": "claude-sonnet-4-20250514"
    }
  }
}
```

### Key Design Decisions
1. **Single interface** — All AI features use `IAiProvider`, never reference a specific provider
2. **Configuration-driven** — Switching providers is a config change, not a code change
3. **Prompt isolation** — Prompts are stored in a dedicated `Prompts/` folder as constants, tested independently
4. **Graceful degradation** — If AI is unavailable, features fall back to non-AI behavior (e.g., standard search)
5. **Cost awareness** — Cache AI responses where appropriate (e.g., book summaries), use smaller models for simple tasks
6. **Rate limiting** — Apply per-user rate limits on AI endpoints to control costs
