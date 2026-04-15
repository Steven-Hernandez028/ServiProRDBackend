using System.ComponentModel.DataAnnotations;

namespace ServiPro.API.Models.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [Required]
    public UserRole Role { get; set; } = UserRole.Client;

    public string? Avatar { get; set; }

    public bool IsActive { get; set; } = true;

    public bool EmailVerified { get; set; } = false;

    public DateTime? EmailVerifiedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Provider? Provider { get; set; }
    public Client? Client { get; set; }
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
}

public enum UserRole
{
    Client = 0,
    Provider = 1,
    Admin = 2
}
