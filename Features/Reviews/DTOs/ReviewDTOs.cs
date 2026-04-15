using System.ComponentModel.DataAnnotations;

namespace ServiPro.API.Features.Reviews.DTOs;

public class ReviewDTO
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? ProviderResponse { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewDTO
{
    [Required]
    public Guid ProviderId { get; set; }

    public Guid? ServiceRequestId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }
}

public class ProviderResponseDTO
{
    [Required]
    [MaxLength(1000)]
    public string Response { get; set; } = string.Empty;
}
