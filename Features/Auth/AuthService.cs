using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServiPro.API.Data;
using ServiPro.API.Features.Auth.DTOs;
using ServiPro.API.Models.Entities;

namespace ServiPro.API.Features.Auth;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RegisterClientAsync(RegisterClientRequest request);
    Task<AuthResponse?> RegisterProviderAsync(RegisterProviderRequest request);
    Task<UserDTO?> GetUserByIdAsync(Guid userId);
}

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        if (!user.IsActive)
            return null;

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new AuthResponse { Token = GenerateJwtToken(user), User = MapToUserDTO(user) };
    }

    public async Task<AuthResponse?> RegisterClientAsync(RegisterClientRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return null;

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Name = request.Name,
            Phone = request.Phone,
            Role = UserRole.Client
        };

        _context.Users.Add(user);

        _context.Clients.Add(new Client
        {
            UserId = user.Id,
            City = request.City,
            Address = request.Address
        });

        await _context.SaveChangesAsync();

        return new AuthResponse { Token = GenerateJwtToken(user), User = MapToUserDTO(user) };
    }

    public async Task<AuthResponse?> RegisterProviderAsync(RegisterProviderRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return null;

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Name = request.Name,
            Phone = request.Phone,
            Role = UserRole.Provider
        };

        _context.Users.Add(user);

        var provider = new Provider
        {
            UserId = user.Id,
            Description = request.Description,
            City = request.City,
            Zone = request.Zone,
            HourlyRate = request.HourlyRate,
            Experience = request.Experience,
            Availability = request.Availability
        };

        _context.Providers.Add(provider);

        // Resolve category codes to IDs
        var categoryCodes = request.Categories.Select(c => c.ToLower()).ToList();
        var categories = await _context.ServiceCategories
            .Where(sc => categoryCodes.Contains(sc.Code.ToLower()))
            .ToListAsync();

        foreach (var category in categories)
        {
            _context.ProviderCategories.Add(new ProviderCategory
            {
                ProviderId = provider.Id,
                CategoryId = category.Id
            });
        }

        await _context.SaveChangesAsync();

        return new AuthResponse { Token = GenerateJwtToken(user), User = MapToUserDTO(user) };
    }

    public async Task<UserDTO?> GetUserByIdAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user == null ? null : MapToUserDTO(user);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDTO MapToUserDTO(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Name = user.Name,
        Phone = user.Phone,
        Role = user.Role,
        Avatar = user.Avatar,
        CreatedAt = user.CreatedAt,
        IsActive = user.IsActive
    };
}
