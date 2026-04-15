using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiPro.API.Features.Auth.DTOs;

namespace ServiPro.API.Features.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
            return Unauthorized(new { message = "Credenciales invalidas" });

        return Ok(result);
    }

    [HttpPost("register/client")]
    public async Task<ActionResult<AuthResponse>> RegisterClient([FromBody] RegisterClientRequest request)
    {
        var result = await _authService.RegisterClientAsync(request);
        if (result == null)
            return BadRequest(new { message = "El email ya esta registrado" });

        return CreatedAtAction(nameof(GetCurrentUser), result);
    }

    [HttpPost("register/provider")]
    public async Task<ActionResult<AuthResponse>> RegisterProvider([FromBody] RegisterProviderRequest request)
    {
        var result = await _authService.RegisterProviderAsync(request);
        if (result == null)
            return BadRequest(new { message = "El email ya esta registrado" });

        return CreatedAtAction(nameof(GetCurrentUser), result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDTO>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(user);
    }
}
