using System.ComponentModel.DataAnnotations;

namespace ServiPro.API.Features.Messages.DTOs;

public class ConversationDTO
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public Guid? ServiceRequestId { get; set; }
    public string? ServiceRequestTitle { get; set; }
    public string? LastMessage { get; set; }
    public DateTime LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public bool IsActive { get; set; }
}

public class ChatMessageDTO
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateMessageDTO
{
    [Required]
    public Guid ConversationId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}

public class StartConversationDTO
{
    [Required]
    public Guid ProviderId { get; set; }

    public Guid? ServiceRequestId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string InitialMessage { get; set; } = string.Empty;
}
