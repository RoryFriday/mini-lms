using Microsoft.EntityFrameworkCore;
using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IBookService
{
    Task<BookDto> CreateAsync(CreateBookDto dto);
    Task<BookDto?> GetByIdAsync(int id);
    Task<PagedResult<BookDto>> SearchAsync(BookSearchDto search);
    Task<BookDto?> UpdateAsync(int id, UpdateBookDto dto);
    Task<bool> DeleteAsync(int id);
}

public class BookService : IBookService
{
    private readonly LibraryDbContext _db;

    public BookService(LibraryDbContext db) => _db = db;

    public async Task<BookDto> CreateAsync(CreateBookDto dto)
    {
        var book = new Book
        {
            Title = dto.Title,
            Author = dto.Author,
            ISBN = dto.ISBN,
            Genre = dto.Genre,
            Description = dto.Description,
            PublicationYear = dto.PublicationYear,
            Publisher = dto.Publisher,
            TotalCopies = dto.TotalCopies,
            AvailableCopies = dto.TotalCopies
        };

        _db.Books.Add(book);
        await _db.SaveChangesAsync();
        return ToDto(book);
    }

    public async Task<BookDto?> GetByIdAsync(int id)
    {
        var book = await _db.Books.FindAsync(id);
        return book == null ? null : ToDto(book);
    }

    public async Task<PagedResult<BookDto>> SearchAsync(BookSearchDto search)
    {
        var query = _db.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search.Query))
        {
            var q = search.Query.ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(q) ||
                b.Author.ToLower().Contains(q) ||
                b.ISBN.ToLower().Contains(q) ||
                b.Genre.ToLower().Contains(q) ||
                b.Description.ToLower().Contains(q));
        }

        if (!string.IsNullOrWhiteSpace(search.Genre))
            query = query.Where(b => b.Genre.ToLower() == search.Genre.ToLower());

        if (search.Available.HasValue)
        {
            query = search.Available.Value
                ? query.Where(b => b.AvailableCopies > 0)
                : query.Where(b => b.AvailableCopies == 0);
        }

        var totalCount = await query.CountAsync();
        var page = Math.Max(1, search.Page);
        var pageSize = Math.Clamp(search.PageSize, 1, 100);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderBy(b => b.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => ToDto(b))
            .ToListAsync();

        return new PagedResult<BookDto>(items, totalCount, page, pageSize, totalPages);
    }

    public async Task<BookDto?> UpdateAsync(int id, UpdateBookDto dto)
    {
        var book = await _db.Books.FindAsync(id);
        if (book == null) return null;

        if (dto.Title != null) book.Title = dto.Title;
        if (dto.Author != null) book.Author = dto.Author;
        if (dto.ISBN != null) book.ISBN = dto.ISBN;
        if (dto.Genre != null) book.Genre = dto.Genre;
        if (dto.Description != null) book.Description = dto.Description;
        if (dto.PublicationYear.HasValue) book.PublicationYear = dto.PublicationYear.Value;
        if (dto.Publisher != null) book.Publisher = dto.Publisher;
        if (dto.TotalCopies.HasValue)
        {
            var diff = dto.TotalCopies.Value - book.TotalCopies;
            book.TotalCopies = dto.TotalCopies.Value;
            book.AvailableCopies = Math.Max(0, book.AvailableCopies + diff);
        }

        book.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(book);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var book = await _db.Books
            .Include(b => b.CheckoutRecords)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (book == null) return false;

        var activeCheckouts = book.CheckoutRecords.Any(c => c.ReturnedAt == null);
        if (activeCheckouts)
            throw new InvalidOperationException("Cannot delete a book with active checkouts.");

        _db.Books.Remove(book);
        await _db.SaveChangesAsync();
        return true;
    }

    private static BookDto ToDto(Book b) =>
        new(b.Id, b.Title, b.Author, b.ISBN, b.Genre, b.Description,
            b.PublicationYear, b.Publisher, b.TotalCopies, b.AvailableCopies,
            b.CreatedAt, b.UpdatedAt);
}
