namespace LibraryApi.Models;

public enum UserRole
{
    Patron,
    Librarian,
    Admin
}

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Patron;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CheckoutRecord> CheckoutRecords { get; set; } = new List<CheckoutRecord>();
}
