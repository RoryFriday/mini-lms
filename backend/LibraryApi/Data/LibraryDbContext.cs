using Microsoft.EntityFrameworkCore;
using LibraryApi.Models;

namespace LibraryApi.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<User> Users => Set<User>();
    public DbSet<CheckoutRecord> CheckoutRecords => Set<CheckoutRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.HasIndex(b => b.ISBN).IsUnique();
            entity.HasIndex(b => b.Title);
            entity.HasIndex(b => b.Author);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<CheckoutRecord>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasOne(c => c.Book)
                  .WithMany(b => b.CheckoutRecords)
                  .HasForeignKey(c => c.BookId);
            entity.HasOne(c => c.User)
                  .WithMany(u => u.CheckoutRecords)
                  .HasForeignKey(c => c.UserId);
        });

        // Seed data
        var adminUser = new User
        {
            Id = 1,
            Email = "admin@library.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            FirstName = "System",
            LastName = "Admin",
            Role = UserRole.Admin,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var librarianUser = new User
        {
            Id = 2,
            Email = "librarian@library.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Librarian123!"),
            FirstName = "Jane",
            LastName = "Librarian",
            Role = UserRole.Librarian,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        modelBuilder.Entity<User>().HasData(adminUser, librarianUser);

        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", ISBN = "978-0743273565", Genre = "Classic Fiction", Description = "A story of decadence and excess.", PublicationYear = 1925, Publisher = "Scribner", TotalCopies = 3, AvailableCopies = 3, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 2, Title = "To Kill a Mockingbird", Author = "Harper Lee", ISBN = "978-0061120084", Genre = "Classic Fiction", Description = "A novel about racial injustice in the Deep South.", PublicationYear = 1960, Publisher = "J. B. Lippincott & Co.", TotalCopies = 2, AvailableCopies = 2, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 3, Title = "1984", Author = "George Orwell", ISBN = "978-0451524935", Genre = "Dystopian", Description = "A dystopian social science fiction novel.", PublicationYear = 1949, Publisher = "Secker & Warburg", TotalCopies = 4, AvailableCopies = 4, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 4, Title = "Pride and Prejudice", Author = "Jane Austen", ISBN = "978-0141439518", Genre = "Romance", Description = "A romantic novel of manners.", PublicationYear = 1813, Publisher = "T. Egerton", TotalCopies = 2, AvailableCopies = 2, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 5, Title = "The Catcher in the Rye", Author = "J.D. Salinger", ISBN = "978-0316769488", Genre = "Coming-of-age", Description = "A story about teenage angst and alienation.", PublicationYear = 1951, Publisher = "Little, Brown", TotalCopies = 3, AvailableCopies = 3, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
