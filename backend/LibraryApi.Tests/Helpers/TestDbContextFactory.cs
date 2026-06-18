using Microsoft.EntityFrameworkCore;
using LibraryApi.Data;
using LibraryApi.Models;

namespace LibraryApi.Tests.Helpers;

public static class TestDbContextFactory
{
    public static LibraryDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        var db = new LibraryDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    public static async Task<LibraryDbContext> CreateWithBooksAsync(string? dbName = null)
    {
        var db = Create(dbName);

        // Clear seeded data to start fresh with controlled test data
        db.Books.RemoveRange(db.Books);
        await db.SaveChangesAsync();

        db.Books.AddRange(
            new Book
            {
                Id = 100, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald",
                ISBN = "test-isbn-1", Genre = "Classic Fiction",
                Description = "A story of decadence and excess.",
                PublicationYear = 1925, Publisher = "Scribner",
                TotalCopies = 3, AvailableCopies = 3
            },
            new Book
            {
                Id = 101, Title = "1984", Author = "George Orwell",
                ISBN = "test-isbn-2", Genre = "Dystopian",
                Description = "A dystopian social science fiction novel.",
                PublicationYear = 1949, Publisher = "Secker & Warburg",
                TotalCopies = 2, AvailableCopies = 2
            },
            new Book
            {
                Id = 102, Title = "Pride and Prejudice", Author = "Jane Austen",
                ISBN = "test-isbn-3", Genre = "Romance",
                Description = "A romantic novel of manners.",
                PublicationYear = 1813, Publisher = "T. Egerton",
                TotalCopies = 1, AvailableCopies = 1
            }
        );

        await db.SaveChangesAsync();
        return db;
    }

    public static async Task<(LibraryDbContext db, User user)> CreateWithBooksAndUserAsync(string? dbName = null)
    {
        var db = await CreateWithBooksAsync(dbName);

        // Clear seeded users
        db.Users.RemoveRange(db.Users);
        await db.SaveChangesAsync();

        var user = new User
        {
            Id = 200,
            Email = "patron@test.com",
            PasswordHash = "hashed",
            FirstName = "Test",
            LastName = "Patron",
            Role = UserRole.Patron
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (db, user);
    }
}
