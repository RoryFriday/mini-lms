using Xunit;
using LibraryApi.Services;
using LibraryApi.Tests.Helpers;

namespace LibraryApi.Tests;

public class CheckoutServiceTests
{
    [Fact]
    public async Task Checkout_DecrementsAvailableCopies()
    {
        var (db, user) = await TestDbContextFactory.CreateWithBooksAndUserAsync();
        var service = new CheckoutService(db);

        var bookBefore = await db.Books.FindAsync(100);
        var copiesBefore = bookBefore!.AvailableCopies;

        await service.CheckoutBookAsync(user.Id, 100);

        var bookAfter = await db.Books.FindAsync(100);
        Assert.Equal(copiesBefore - 1, bookAfter!.AvailableCopies);
    }

    [Fact]
    public async Task Checkout_SameBookTwice_Throws()
    {
        var (db, user) = await TestDbContextFactory.CreateWithBooksAndUserAsync();
        var service = new CheckoutService(db);

        await service.CheckoutBookAsync(user.Id, 100);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CheckoutBookAsync(user.Id, 100));
        Assert.Contains("already have this book checked out", ex.Message);
    }

    [Fact]
    public async Task Checkout_NoAvailableCopies_Throws()
    {
        var (db, user) = await TestDbContextFactory.CreateWithBooksAndUserAsync();
        var service = new CheckoutService(db);

        // Book 102 has only 1 copy — check it out first
        await service.CheckoutBookAsync(user.Id, 102);

        // Add a second user to attempt checkout
        var user2 = new LibraryApi.Models.User
        {
            Id = 201, Email = "patron2@test.com", PasswordHash = "hashed",
            FirstName = "Test2", LastName = "Patron2", Role = LibraryApi.Models.UserRole.Patron
        };
        db.Users.Add(user2);
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CheckoutBookAsync(user2.Id, 102));
        Assert.Contains("No copies available", ex.Message);
    }

    [Fact]
    public async Task Checkout_NonexistentBook_Throws()
    {
        var (db, user) = await TestDbContextFactory.CreateWithBooksAndUserAsync();
        var service = new CheckoutService(db);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CheckoutBookAsync(user.Id, 9999));
        Assert.Contains("Book not found", ex.Message);
    }

    [Fact]
    public async Task Return_IncrementsAvailableCopies()
    {
        var (db, user) = await TestDbContextFactory.CreateWithBooksAndUserAsync();
        var service = new CheckoutService(db);

        var record = await service.CheckoutBookAsync(user.Id, 100);
        var copiesAfterCheckout = (await db.Books.FindAsync(100))!.AvailableCopies;

        await service.ReturnBookAsync(user.Id, record.Id);

        var bookAfter = await db.Books.FindAsync(100);
        Assert.Equal(copiesAfterCheckout + 1, bookAfter!.AvailableCopies);
    }

    [Fact]
    public async Task Return_SetsReturnedAt()
    {
        var (db, user) = await TestDbContextFactory.CreateWithBooksAndUserAsync();
        var service = new CheckoutService(db);

        var record = await service.CheckoutBookAsync(user.Id, 100);
        Assert.False(record.IsReturned);

        var returned = await service.ReturnBookAsync(user.Id, record.Id);
        Assert.True(returned.IsReturned);
        Assert.NotNull(returned.ReturnedAt);
    }

    [Fact]
    public async Task Return_AlreadyReturned_Throws()
    {
        var (db, user) = await TestDbContextFactory.CreateWithBooksAndUserAsync();
        var service = new CheckoutService(db);

        var record = await service.CheckoutBookAsync(user.Id, 100);
        await service.ReturnBookAsync(user.Id, record.Id);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ReturnBookAsync(user.Id, record.Id));
        Assert.Contains("already been returned", ex.Message);
    }

    [Fact]
    public async Task Return_ThenCheckoutAgain_Succeeds()
    {
        var (db, user) = await TestDbContextFactory.CreateWithBooksAndUserAsync();
        var service = new CheckoutService(db);

        var record = await service.CheckoutBookAsync(user.Id, 100);
        await service.ReturnBookAsync(user.Id, record.Id);

        // Should be able to check out again after returning
        var record2 = await service.CheckoutBookAsync(user.Id, 100);
        Assert.False(record2.IsReturned);
    }
}
