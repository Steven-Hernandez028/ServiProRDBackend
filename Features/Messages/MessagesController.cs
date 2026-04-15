using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiPro.API.Features.Messages.DTOs;

namespace ServiPro.API.Features.Messages;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<List<ConversationDTO>>> GetConversations()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var conversations = await _messageService.GetConversationsForUserAsync(userId.Value);
        return Ok(conversations);
    }

    [HttpGet("conversations/{conversationId:guid}/messages")]
    public async Task<ActionResult<List<ChatMessageDTO>>> GetMessages(Guid conversationId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var messages = await _messageService.GetMessagesAsync(conversationId, userId.Value);
        return Ok(messages);
    }

    [HttpPost]
    public async Task<ActionResult<ChatMessageDTO>> SendMessage([FromBody] CreateMessageDTO dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var message = await _messageService.SendMessageAsync(userId.Value, dto);
        if (message == null)
            return BadRequest(new { message = "No se pudo enviar el mensaje" });

        return Ok(message);
    }

    [Authorize(Roles = "Client")]
    [HttpPost("conversations")]
    public async Task<ActionResult<ConversationDTO>> StartConversation([FromBody] StartConversationDTO dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var conversation = await _messageService.StartConversationAsync(userId.Value, dto);
        if (conversation == null)
            return BadRequest(new { message = "No se pudo iniciar la conversacion" });

        return CreatedAtAction(nameof(GetConversations), conversation);
    }

    [HttpPost("conversations/{conversationId:guid}/read")]
    public async Task<ActionResult> MarkAsRead(Guid conversationId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        await _messageService.MarkAsReadAsync(conversationId, userId.Value);
        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
