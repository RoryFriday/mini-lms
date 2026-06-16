using Microsoft.EntityFrameworkCore;
using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface ICheckoutService
{
    Task<CheckoutRecordDto> CheckoutBookAsync(int userId, int bookId);
    Task<CheckoutRecordDto> ReturnBookAsync(int userId, int recordId);
    Task<IEnumerable<CheckoutRecordDto>> GetUserCheckoutsAsync(int userId, bool activeOnly = false);
    Task<IEnumerable<CheckoutRecordDto>> GetAllCheckoutsAsync(bool activeOnly = false);
    Task<IEnumerable<CheckoutRecordDto>> GetBookCheckoutsAsync(int bookId);
}

public class CheckoutService : ICheckoutService
{
    private readonly LibraryDbContext _db;

    public CheckoutService(LibraryDbContext db) => _db = db;

    public async Task<CheckoutRecordDto> CheckoutBookAsync(int userId, int bookId)
    {
        var book = await _db.Books.FindAsync(bookId)
            ?? throw new InvalidOperationException("Book not found.");

        if (book.AvailableCopies <= 0)
            throw new InvalidOperationException("No copies available for checkout.");

        // Check if user already has this book checked out
        var existing = await _db.CheckoutRecords
            .AnyAsync(c => c.UserId == userId && c.BookId == bookId && c.ReturnedAt == null);
        if (existing)
            throw new InvalidOperationException("You already have this book checked out.");

        book.AvailableCopies--;
        book.UpdatedAt = DateTime.UtcNow;

        var record = new CheckoutRecord
        {
            BookId = bookId,
            UserId = userId,
            CheckedOutAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14)
        };

        _db.CheckoutRecords.Add(record);
        await _db.SaveChangesAsync();

        return await GetRecordDtoAsync(record.Id);
    }

    public async Task<CheckoutRecordDto> ReturnBookAsync(int userId, int recordId)
    {
        var record = await _db.CheckoutRecords
            .Include(c => c.Book)
            .FirstOrDefaultAsync(c => c.Id == recordId)
            ?? throw new InvalidOperationException("Checkout record not found.");

        if (record.ReturnedAt != null)
            throw new InvalidOperationException("This book has already been returned.");

        // Only the borrower or a librarian/admin can return (handled at controller level)

        record.ReturnedAt = DateTime.UtcNow;
        record.Book.AvailableCopies++;
        record.Book.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetRecordDtoAsync(record.Id);
    }

    public async Task<IEnumerable<CheckoutRecordDto>> GetUserCheckoutsAsync(int userId, bool activeOnly = false)
    {
        var query = _db.CheckoutRecords
            .Include(c => c.Book)
            .Include(c => c.User)
            .Where(c => c.UserId == userId);

        if (activeOnly)
            query = query.Where(c => c.ReturnedAt == null);

        return await query.OrderByDescending(c => c.CheckedOutAt)
            .Select(c => ToDto(c))
            .ToListAsync();
    }

    public async Task<IEnumerable<CheckoutRecordDto>> GetAllCheckoutsAsync(bool activeOnly = false)
    {
        var query = _db.CheckoutRecords
            .Include(c => c.Book)
            .Include(c => c.User)
            .AsQueryable();

        if (activeOnly)
            query = query.Where(c => c.ReturnedAt == null);

        return await query.OrderByDescending(c => c.CheckedOutAt)
            .Select(c => ToDto(c))
            .ToListAsync();
    }

    public async Task<IEnumerable<CheckoutRecordDto>> GetBookCheckoutsAsync(int bookId)
    {
        return await _db.CheckoutRecords
            .Include(c => c.Book)
            .Include(c => c.User)
            .Where(c => c.BookId == bookId)
            .OrderByDescending(c => c.CheckedOutAt)
            .Select(c => ToDto(c))
            .ToListAsync();
    }

    private async Task<CheckoutRecordDto> GetRecordDtoAsync(int id)
    {
        var record = await _db.CheckoutRecords
            .Include(c => c.Book)
            .Include(c => c.User)
            .FirstAsync(c => c.Id == id);
        return ToDto(record);
    }

    private static CheckoutRecordDto ToDto(CheckoutRecord c) =>
        new(c.Id, c.BookId, c.Book.Title, c.Book.Author,
            c.UserId, c.User.Email, c.CheckedOutAt, c.DueDate,
            c.ReturnedAt, c.ReturnedAt.HasValue);
}
