using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<bool> UpdateUserRoleAsync(int adminUserId, int userId, UserRole role);
}

public class AuthService : IAuthService
{
    private readonly LibraryDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(LibraryDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
            throw new InvalidOperationException("Password must be at least 8 characters.");

        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = UserRole.Patron
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = GenerateToken(user);
        return new AuthResponseDto(token, user.Email, user.FirstName, user.LastName, user.Role.ToString());
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = GenerateToken(user);
        return new AuthResponseDto(token, user.Email, user.FirstName, user.LastName, user.Role.ToString());
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        return user == null ? null : ToDto(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        return await _db.Users.Select(u => ToDto(u)).ToListAsync();
    }

    public async Task<bool> UpdateUserRoleAsync(int adminUserId, int userId, UserRole role)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return false;

        // Prevent admin from demoting themselves
        if (userId == adminUserId)
            throw new InvalidOperationException("You cannot change your own role.");

        // Prevent removing the last admin
        if (user.Role == UserRole.Admin && role != UserRole.Admin)
        {
            var adminCount = await _db.Users.CountAsync(u => u.Role == UserRole.Admin);
            if (adminCount <= 1)
                throw new InvalidOperationException("Cannot demote the last admin.");
        }

        user.Role = role;
        await _db.SaveChangesAsync();
        return true;
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["Jwt:Key"] ?? "SuperSecretKeyThatIsLongEnough1234567890!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "LibraryApi",
            audience: _config["Jwt:Audience"] ?? "LibraryApp",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDto ToDto(User u) =>
        new(u.Id, u.Email, u.FirstName, u.LastName, u.Role.ToString(), u.CreatedAt);
}
