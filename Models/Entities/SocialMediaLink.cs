using System.ComponentModel.DataAnnotations;

namespace ServiPro.API.Models.Entities;

public class SocialMediaLink
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string Platform { get; set; } = string.Empty;

    [Required]
    public string Url { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Label { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int Order { get; set; } = 0;

    [MaxLength(20)]
    public string? Followers { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
