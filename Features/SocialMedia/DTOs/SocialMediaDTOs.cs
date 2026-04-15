using System.ComponentModel.DataAnnotations;

namespace ServiPro.API.Features.SocialMedia.DTOs;

public class SocialMediaLinkDTO
{
    public Guid Id { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public string? Followers { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateSocialMediaLinkDTO
{
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
}

public class UpdateSocialMediaLinkDTO
{
    public string? Url { get; set; }

    [MaxLength(200)]
    public string? Label { get; set; }

    public bool? IsActive { get; set; }

    public int? Order { get; set; }

    [MaxLength(20)]
    public string? Followers { get; set; }
}
