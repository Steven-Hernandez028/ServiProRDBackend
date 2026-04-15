using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiPro.API.Models.Entities;

public class ServiceRequest
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ClientId { get; set; }

    [ForeignKey(nameof(ClientId))]
    public Client Client { get; set; } = null!;

    public Guid? ProviderId { get; set; }

    [ForeignKey(nameof(ProviderId))]
    public Provider? Provider { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public ServiceCategory Category { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Zone { get; set; }

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    [Column(TypeName = "decimal(12,2)")]
    public decimal? Budget { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? FinalPrice { get; set; }

    public DateOnly? PreferredDate { get; set; }

    public TimeOnly? PreferredTimeStart { get; set; }

    public TimeOnly? PreferredTimeEnd { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }

    public string[]? Images { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum RequestStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Completed = 3,
    Cancelled = 4
}
