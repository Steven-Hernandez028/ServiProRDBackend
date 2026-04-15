using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiPro.API.Models.Entities;

public class ProviderCategory
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid ProviderId { get; set; }

    [ForeignKey(nameof(ProviderId))]
    public Provider Provider { get; set; } = null!;

    [Required]
    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public ServiceCategory Category { get; set; } = null!;

    public int? YearsExperience { get; set; }

    public bool IsPrimary { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
