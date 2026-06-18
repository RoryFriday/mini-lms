using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using LibraryApi.Services.Ai;
using LibraryApi.Tests.Helpers;

namespace LibraryApi.Tests;

public class AiSearchServiceTests
{
    private static Mock<IAiProvider> CreateMockProvider(string response)
    {
        var mock = new Mock<IAiProvider>();
        mock.Setup(p => p.IsConfigured).Returns(true);
        mock.Setup(p => p.ChatAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(response);
        return mock;
    }

    private static ILogger<AiSearchService> CreateLogger() =>
        new Mock<ILogger<AiSearchService>>().Object;

    [Fact]
    public async Task Search_MalformedJson_ReturnsEmptyResult()
    {
        var db = await TestDbContextFactory.CreateWithBooksAsync();
        var provider = CreateMockProvider("this is not json at all!!!");
        var service = new AiSearchService(provider.Object, db, CreateLogger());

        var result = await service.SearchAsync("find me a book");

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task Search_PartiallyInvalidJson_ReturnsEmptyResult()
    {
        var db = await TestDbContextFactory.CreateWithBooksAsync();
        var provider = CreateMockProvider("[{\"id\": 1, \"score\": broken}]");
        var service = new AiSearchService(provider.Object, db, CreateLogger());

        var result = await service.SearchAsync("find me a book");

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task Search_EmptyArray_ReturnsEmptyResult()
    {
        var db = await TestDbContextFactory.CreateWithBooksAsync();
        var provider = CreateMockProvider("[]");
        var service = new AiSearchService(provider.Object, db, CreateLogger());

        var result = await service.SearchAsync("find me a book");

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task Search_ValidJson_ReturnsMatchedBooks()
    {
        var db = await TestDbContextFactory.CreateWithBooksAsync();
        var provider = CreateMockProvider(
            """[{"id":101,"score":0.95,"reason":"Matches dystopian theme."}]""");
        var service = new AiSearchService(provider.Object, db, CreateLogger());

        var result = await service.SearchAsync("dystopian surveillance");

        Assert.Single(result.Items);
        Assert.Equal("1984", result.Items.First().Book.Title);
        Assert.Equal(0.95, result.Items.First().Score);
        Assert.Equal("Matches dystopian theme.", result.Items.First().Reason);
    }

    [Fact]
    public async Task Search_ValidJsonWithCodeFences_StripsAndParses()
    {
        var db = await TestDbContextFactory.CreateWithBooksAsync();
        var response = "```json\n[{\"id\":100,\"score\":0.8,\"reason\":\"Classic novel.\"}]\n```";
        var provider = CreateMockProvider(response);
        var service = new AiSearchService(provider.Object, db, CreateLogger());

        var result = await service.SearchAsync("classic novel");

        Assert.Single(result.Items);
        Assert.Equal("The Great Gatsby", result.Items.First().Book.Title);
    }

    [Fact]
    public async Task Search_ReferencesNonexistentBookId_IgnoresIt()
    {
        var db = await TestDbContextFactory.CreateWithBooksAsync();
        var provider = CreateMockProvider(
            """[{"id":9999,"score":0.9,"reason":"Ghost book."},{"id":101,"score":0.7,"reason":"Real match."}]""");
        var service = new AiSearchService(provider.Object, db, CreateLogger());

        var result = await service.SearchAsync("anything");

        Assert.Single(result.Items);
        Assert.Equal("1984", result.Items.First().Book.Title);
    }

    [Fact]
    public async Task Search_ProviderNotConfigured_Throws()
    {
        var db = await TestDbContextFactory.CreateWithBooksAsync();
        var provider = new Mock<IAiProvider>();
        provider.Setup(p => p.IsConfigured).Returns(false);
        var service = new AiSearchService(provider.Object, db, CreateLogger());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SearchAsync("anything"));
    }

    [Fact]
    public async Task Search_HtmlResponse_ReturnsEmptyResult()
    {
        var db = await TestDbContextFactory.CreateWithBooksAsync();
        var provider = CreateMockProvider("<html><body>Error 500</body></html>");
        var service = new AiSearchService(provider.Object, db, CreateLogger());

        var result = await service.SearchAsync("find me a book");

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
}
