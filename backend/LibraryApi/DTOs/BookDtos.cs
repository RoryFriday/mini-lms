namespace LibraryApi.DTOs;

public record CreateBookDto(
    string Title,
    string Author,
    string ISBN,
    string Genre,
    string Description,
    int PublicationYear,
    string Publisher,
    int TotalCopies
);

public record UpdateBookDto(
    string? Title,
    string? Author,
    string? ISBN,
    string? Genre,
    string? Description,
    int? PublicationYear,
    string? Publisher,
    int? TotalCopies
);

public record BookDto(
    int Id,
    string Title,
    string Author,
    string ISBN,
    string Genre,
    string Description,
    int PublicationYear,
    string Publisher,
    int TotalCopies,
    int AvailableCopies,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record BookSearchDto(
    string? Query,
    string? Genre,
    bool? Available,
    int Page = 1,
    int PageSize = 20
);

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
