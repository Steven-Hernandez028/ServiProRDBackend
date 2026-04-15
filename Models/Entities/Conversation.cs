using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiPro.API.Models.Entities;

public class Conversation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ClientId { get; set; }

    [ForeignKey(nameof(ClientId))]
    public Client Client { get; set; } = null!;

    [Required]
    public Guid ProviderId { get; set; }

    [ForeignKey(nameof(ProviderId))]
    public Provider Provider { get; set; } = null!;

    public Guid? ServiceRequestId { get; set; }

    [ForeignKey(nameof(ServiceRequestId))]
    public ServiceRequest? ServiceRequest { get; set; }

    public string? LastMessage { get; set; }

    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    public int ClientUnreadCount { get; set; } = 0;

    public int ProviderUnreadCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
