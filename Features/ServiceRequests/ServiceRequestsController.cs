using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiPro.API.Features.ServiceRequests.DTOs;
using ServiPro.API.Models.Entities;
using ServiPro.API.Shared.DTOs;

namespace ServiPro.API.Features.ServiceRequests;

[ApiController]
[Route("api/servicerequests")]
[Authorize]
public class ServiceRequestsController : ControllerBase
{
    private readonly IServiceRequestService _requestService;

    public ServiceRequestsController(IServiceRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ServiceRequestDTO>> CreateRequest([FromBody] CreateServiceRequestDTO request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _requestService.CreateRequestAsync(userId.Value, request);
        if (result == null)
            return BadRequest(new { message = "No se pudo crear la solicitud" });

        return CreatedAtAction(nameof(GetRequest), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceRequestDTO>> GetRequest(Guid id)
    {
        var request = await _requestService.GetRequestByIdAsync(id);
        if (request == null)
            return NotFound();

        return Ok(request);
    }
    //TODO: implementar un delete para eliminar con soft delete el service request

    [HttpGet("client")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<PagedResult<ServiceRequestDTO>>> GetMyClientRequests([FromQuery] ServiceRequestSearchDTO search)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        return Ok(await _requestService.GetRequestsForClientAsync(userId.Value, search));
    }

    [HttpGet("provider")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult<PagedResult<ServiceRequestDTO>>> GetMyProviderRequests([FromQuery] ServiceRequestSearchDTO search)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        return Ok(await _requestService.GetRequestsForProviderAsync(userId.Value, search));
    }

    [HttpGet("available")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult<PagedResult<ServiceRequestDTO>>> GetAvailableRequests([FromQuery] ServiceRequestSearchDTO search)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        return Ok(await _requestService.GetAvailableRequestsAsync(userId.Value, search));
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<ServiceRequestDTO>>> GetAllRequests([FromQuery] ServiceRequestSearchDTO search)
    {
        return Ok(await _requestService.GetAllRequestsAsync(search));
    }

    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult> AcceptRequest(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var success = await _requestService.AcceptRequestAsync(id, userId.Value);
        if (!success)
            return BadRequest(new { message = "No se pudo aceptar la solicitud" });

        return NoContent();
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult> UpdateRequestStatus(Guid id, [FromBody] UpdateServiceRequestStatusDTO request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var success = await _requestService.UpdateRequestStatusAsync(id, userId.Value, request.Status);
        if (!success)
            return BadRequest(new { message = "No se pudo actualizar el estado" });

        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
