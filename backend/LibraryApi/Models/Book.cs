namespace LibraryApi.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PublicationYear { get; set; }
    public string Publisher { get; set; } = string.Empty;
    public int TotalCopies { get; set; } = 1;
    public int AvailableCopies { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CheckoutRecord> CheckoutRecords { get; set; } = new List<CheckoutRecord>();
}
