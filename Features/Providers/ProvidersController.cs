using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiPro.API.Features.Providers.DTOs;
using ServiPro.API.Shared.DTOs;

namespace ServiPro.API.Features.Providers;

[ApiController]
[Route("api/providers")]
public class ProvidersController : ControllerBase
{
    private readonly IProviderService _providerService;

    public ProvidersController(IProviderService providerService)
    {
        _providerService = providerService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProviderListDTO>>> SearchProviders([FromQuery] ProviderSearchRequest request)
    {
        var result = await _providerService.SearchProvidersAsync(request);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProviderDTO>> GetProvider(Guid id)
    {
        var provider = await _providerService.GetProviderByIdAsync(id);
        if (provider == null)
            return NotFound();

        return Ok(provider);
    }

    [Authorize(Roles = "Provider")]
    [HttpGet("me")]
    public async Task<ActionResult<ProviderDTO>> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var provider = await _providerService.GetProviderByUserIdAsync(userId.Value);
        if (provider == null)
            return NotFound();

        return Ok(provider);
    }

    [Authorize(Roles = "Provider")]
    [HttpPut("me")]
    public async Task<ActionResult<ProviderDTO>> UpdateMyProfile([FromBody] UpdateProviderRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var provider = await _providerService.UpdateProviderAsync(userId.Value, request);
        if (provider == null)
            return NotFound();

        return Ok(provider);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<ActionResult<List<ProviderDTO>>> GetAllProviders()
    {
        var providers = await _providerService.GetAllProvidersAsync();
        return Ok(providers);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/verify")]
    public async Task<ActionResult> VerifyProvider(Guid id)
    {
        var success = await _providerService.VerifyProviderAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/toggle-active")]
    public async Task<ActionResult> ToggleProviderActive(Guid id)
    {
        var success = await _providerService.ToggleProviderActiveAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
