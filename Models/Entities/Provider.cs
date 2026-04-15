using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiPro.API.Models.Entities;

public class Provider
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Zone { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal Rating { get; set; } = 0;

    public int ReviewCount { get; set; } = 0;

    [Column(TypeName = "decimal(10,2)")]
    public decimal? HourlyRate { get; set; }

    public bool IsVerified { get; set; } = false;

    public DateTime? VerifiedAt { get; set; }

    [MaxLength(100)]
    public string? Experience { get; set; }

    public string? Availability { get; set; }

    public string[]? PortfolioImages { get; set; }

    [MaxLength(200)]
    public string? BusinessName { get; set; }

    [MaxLength(50)]
    public string? TaxId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ProviderCategory> Categories { get; set; } = new List<ProviderCategory>();
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
