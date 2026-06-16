namespace LibraryApi.DTOs;

public record CheckoutDto(int BookId);

public record CheckoutRecordDto(
    int Id,
    int BookId,
    string BookTitle,
    string BookAuthor,
    int UserId,
    string UserEmail,
    DateTime CheckedOutAt,
    DateTime DueDate,
    DateTime? ReturnedAt,
    bool IsReturned
);
