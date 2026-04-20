using System.ComponentModel.DataAnnotations;
using ServiPro.API.Models.Entities;

namespace ServiPro.API.Features.Auth.DTOs;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterClientRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }
}

public class RegisterProviderRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Zone { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Category codes e.g. ["electricista", "plomero"]</summary>
    [Required]
    [MinLength(1)]
    public List<string> Categories { get; set; } = new();

    public decimal? HourlyRate { get; set; }

    [MaxLength(100)]
    public string? Experience { get; set; }

    [MaxLength(200)]
    public string? Availability { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDTO User { get; set; } = null!;
}

public class UserDTO
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public string? Avatar { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
