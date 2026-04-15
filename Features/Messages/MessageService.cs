using Microsoft.EntityFrameworkCore;
using ServiPro.API.Data;
using ServiPro.API.Features.Messages.DTOs;
using ServiPro.API.Models.Entities;

namespace ServiPro.API.Features.Messages;

public interface IMessageService
{
    Task<List<ConversationDTO>> GetConversationsForUserAsync(Guid userId);
    Task<List<ChatMessageDTO>> GetMessagesAsync(Guid conversationId, Guid userId);
    Task<ChatMessageDTO?> SendMessageAsync(Guid userId, CreateMessageDTO dto);
    Task<ConversationDTO?> StartConversationAsync(Guid clientUserId, StartConversationDTO dto);
    Task MarkAsReadAsync(Guid conversationId, Guid userId);
}

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _context;

    public MessageService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ConversationDTO>> GetConversationsForUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return new();

        IQueryable<Conversation> query;

        if (user.Role == UserRole.Client)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);
            if (client == null) return new();
            query = _context.Conversations
                .Include(c => c.Client).ThenInclude(cl => cl.User)
                .Include(c => c.Provider).ThenInclude(p => p.User)
                .Include(c => c.ServiceRequest)
                .Where(c => c.ClientId == client.Id);
        }
        else
        {
            var provider = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
            if (provider == null) return new();
            query = _context.Conversations
                .Include(c => c.Client).ThenInclude(cl => cl.User)
                .Include(c => c.Provider).ThenInclude(p => p.User)
                .Include(c => c.ServiceRequest)
                .Where(c => c.ProviderId == provider.Id);
        }

        var conversations = await query
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();

        return conversations.Select(c => MapConversationToDTO(c, user.Role)).ToList();
    }

    public async Task<List<ChatMessageDTO>> GetMessagesAsync(Guid conversationId, Guid userId)
    {
        // Verify user belongs to conversation
        var conversation = await _context.Conversations
            .Include(c => c.Client).ThenInclude(cl => cl.User)
            .Include(c => c.Provider).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null) return new();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return new();

        bool isParticipant = (user.Role == UserRole.Client && conversation.Client.UserId == userId) ||
                             (user.Role == UserRole.Provider && conversation.Provider.UserId == userId) ||
                             user.Role == UserRole.Admin;

        if (!isParticipant) return new();

        var messages = await _context.ChatMessages
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return messages.Select(MapMessageToDTO).ToList();
    }

    public async Task<ChatMessageDTO?> SendMessageAsync(Guid userId, CreateMessageDTO dto)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Client).ThenInclude(cl => cl.User)
            .Include(c => c.Provider).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(c => c.Id == dto.ConversationId);

        if (conversation == null) return null;

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        string senderRole;
        if (user.Role == UserRole.Client && conversation.Client.UserId == userId)
        {
            senderRole = "client";
            conversation.ProviderUnreadCount++;
        }
        else if (user.Role == UserRole.Provider && conversation.Provider.UserId == userId)
        {
            senderRole = "provider";
            conversation.ClientUnreadCount++;
        }
        else return null;

        var message = new ChatMessage
        {
            ConversationId = dto.ConversationId,
            SenderId = userId,
            SenderRole = senderRole,
            Content = dto.Content
        };

        _context.ChatMessages.Add(message);

        conversation.LastMessage = dto.Content;
        conversation.LastMessageAt = DateTime.UtcNow;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var saved = await _context.ChatMessages
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == message.Id);

        return saved == null ? null : MapMessageToDTO(saved);
    }

    public async Task<ConversationDTO?> StartConversationAsync(Guid clientUserId, StartConversationDTO dto)
    {
        var client = await _context.Clients
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (client == null) return null;

        var provider = await _context.Providers
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == dto.ProviderId);
        if (provider == null) return null;

        // Check existing conversation for same request
        Conversation? existing = null;
        if (dto.ServiceRequestId.HasValue)
        {
            existing = await _context.Conversations
                .FirstOrDefaultAsync(c => c.ClientId == client.Id &&
                                          c.ProviderId == dto.ProviderId &&
                                          c.ServiceRequestId == dto.ServiceRequestId);
        }

        if (existing == null)
        {
            existing = new Conversation
            {
                ClientId = client.Id,
                ProviderId = dto.ProviderId,
                ServiceRequestId = dto.ServiceRequestId,
                LastMessage = dto.InitialMessage,
                LastMessageAt = DateTime.UtcNow,
                ProviderUnreadCount = 1
            };
            _context.Conversations.Add(existing);
            await _context.SaveChangesAsync();
        }

        var message = new ChatMessage
        {
            ConversationId = existing.Id,
            SenderId = clientUserId,
            SenderRole = "client",
            Content = dto.InitialMessage
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        return await GetConversationDTOAsync(existing.Id, UserRole.Client);
    }

    public async Task MarkAsReadAsync(Guid conversationId, Guid userId)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Client).ThenInclude(cl => cl.User)
            .Include(c => c.Provider).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null) return;

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;

        if (user.Role == UserRole.Client && conversation.Client.UserId == userId)
            conversation.ClientUnreadCount = 0;
        else if (user.Role == UserRole.Provider && conversation.Provider.UserId == userId)
            conversation.ProviderUnreadCount = 0;

        await _context.SaveChangesAsync();
    }

    private async Task<ConversationDTO?> GetConversationDTOAsync(Guid conversationId, UserRole viewerRole)
    {
        var c = await _context.Conversations
            .Include(conv => conv.Client).ThenInclude(cl => cl.User)
            .Include(conv => conv.Provider).ThenInclude(p => p.User)
            .Include(conv => conv.ServiceRequest)
            .FirstOrDefaultAsync(conv => conv.Id == conversationId);

        return c == null ? null : MapConversationToDTO(c, viewerRole);
    }

    private static ConversationDTO MapConversationToDTO(Conversation c, UserRole viewerRole) => new()
    {
        Id = c.Id,
        ClientId = c.ClientId,
        ClientName = c.Client.User.Name,
        ProviderId = c.ProviderId,
        ProviderName = c.Provider.User.Name,
        ServiceRequestId = c.ServiceRequestId,
        ServiceRequestTitle = c.ServiceRequest?.Title,
        LastMessage = c.LastMessage,
        LastMessageAt = c.LastMessageAt,
        UnreadCount = viewerRole == UserRole.Client ? c.ClientUnreadCount : c.ProviderUnreadCount,
        IsActive = c.IsActive
    };

    private static ChatMessageDTO MapMessageToDTO(ChatMessage m) => new()
    {
        Id = m.Id,
        ConversationId = m.ConversationId,
        SenderId = m.SenderId,
        SenderName = m.Sender.Name,
        SenderRole = m.SenderRole,
        Content = m.Content,
        IsRead = m.IsRead,
        CreatedAt = m.CreatedAt
    };
}
