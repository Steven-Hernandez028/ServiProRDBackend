using System.ComponentModel.DataAnnotations;

namespace ServiPro.API.Features.Advertisements.DTOs;

public class AdvertisementDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string LinkUrl { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Impressions { get; set; }
    public int Clicks { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Advertiser { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateAdvertisementDTO
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public string ImageUrl { get; set; } = string.Empty;

    [Required]
    public string LinkUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Position { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    [MaxLength(200)]
    public string Advertiser { get; set; } = string.Empty;
}

public class UpdateAdvertisementDTO
{
    [MaxLength(200)]
    public string? Title { get; set; }

    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? LinkUrl { get; set; }

    [MaxLength(50)]
    public string? Position { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [MaxLength(200)]
    public string? Advertiser { get; set; }
}
