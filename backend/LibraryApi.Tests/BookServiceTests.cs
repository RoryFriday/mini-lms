using Xunit;
using LibraryApi.DTOs;
using LibraryApi.Services;
using LibraryApi.Tests.Helpers;

namespace LibraryApi.Tests;

public class BookServiceTests
{
    [Fact]
    public async Task Search_FiltersByTitle()
    {
        var db = await TestDbContextFactory.CreateWithBooksAsync();
        var service = new BookService(db);

        var result = await service.SearchAsync(new BookSearchDto("Gatsby", null, null));

        Assert.Single(result.Items);
        Assert.Equal("The Great Gatsby", result.Items.First().Title);
    }

    [Fact]
    public async Task Search_FiltersByAuthor()
    {
        var db = await TestDbContextFactory.CreateWithBooksAsync();
        var service = new BookService(db);

        var result = await service.SearchAsync(new BookSearchDto("Orwell", null, null));

        Assert.Single(result.Items);
        Assert.Equal("1984", result.Items.First().Title);
    }

    [Fact]
    public async Task Search_FiltersByGenre()
    {
        var db = await TestDbContextFactory.CreateWithBooksAsync();
        var service = new BookService(db);

        var result = await service.SearchAsync(new BookSearchDto(null, "Romance", null));

        Assert.Single(result.Items);
        Assert.Equal("Pride and Prejudice", result.Items.First().Title);
    }

    [Fact]
    public async Task Search_QueryMatchesMultipleFields()
    {
        var db = await TestDbContextFactory.CreateWithBooksAsync();
        var service = new BookService(db);

        // "fiction" appears in genre of Great Gatsby ("Classic Fiction") and description won't match Dystopian
        var result = await service.SearchAsync(new BookSearchDto("dystopian", null, null));

        Assert.Contains(result.Items, b => b.Title == "1984");
    }

    [Fact]
    public async Task Search_ReturnsAllWhenNoFilters()
    {
        var db = await TestDbContextFactory.CreateWithBooksAsync();
        var service = new BookService(db);

        var result = await service.SearchAsync(new BookSearchDto(null, null, null));

        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task Search_PaginationBeyondAvailableResults_ReturnsEmpty()
    {
        var db = await TestDbContextFactory.CreateWithBooksAsync();
        var service = new BookService(db);

        var result = await service.SearchAsync(new BookSearchDto(null, null, null, Page: 999, PageSize: 10));

        Assert.Empty(result.Items);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task Search_PaginationZeroOrNegativePage_DefaultsToPageOne()
    {
        var db = await TestDbContextFactory.CreateWithBooksAsync();
        var service = new BookService(db);

        var result = await service.SearchAsync(new BookSearchDto(null, null, null, Page: 0, PageSize: 2));

        Assert.Equal(2, result.Items.Count());
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public async Task Search_PaginationCorrectlySplitsResults()
    {
        var db = await TestDbContextFactory.CreateWithBooksAsync();
        var service = new BookService(db);

        var page1 = await service.SearchAsync(new BookSearchDto(null, null, null, Page: 1, PageSize: 2));
        var page2 = await service.SearchAsync(new BookSearchDto(null, null, null, Page: 2, PageSize: 2));

        Assert.Equal(2, page1.Items.Count());
        Assert.Single(page2.Items);
        Assert.Equal(2, page1.TotalPages);

        // No overlap between pages
        var page1Ids = page1.Items.Select(b => b.Id).ToHashSet();
        var page2Ids = page2.Items.Select(b => b.Id).ToHashSet();
        Assert.Empty(page1Ids.Intersect(page2Ids));
    }

    [Fact]
    public async Task Search_NoMatch_ReturnsEmptyWithZeroCount()
    {
        var db = await TestDbContextFactory.CreateWithBooksAsync();
        var service = new BookService(db);

        var result = await service.SearchAsync(new BookSearchDto("xyznonexistent", null, null));

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
}
