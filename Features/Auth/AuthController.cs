using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiPro.API.Features.Auth.DTOs;

namespace ServiPro.API.Features.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string CookieName = "auth_token";
    private const int CookieLifetimeDays = 7;

    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _env;

    public AuthController(IAuthService authService, IWebHostEnvironment env)
    {
        _authService = authService;
        _env = env;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
            return Unauthorized(new { message = "Credenciales invalidas" });

        SetAuthCookie(result.Token);
        return Ok(result.User);
    }

    [HttpPost("register/client")]
    public async Task<ActionResult<UserDTO>> RegisterClient([FromBody] RegisterClientRequest request)
    {
        var result = await _authService.RegisterClientAsync(request);
        if (result == null)
            return BadRequest(new { message = "El email ya esta registrado" });

        SetAuthCookie(result.Token);
        return CreatedAtAction(nameof(GetCurrentUser), result.User);
    }

    [HttpPost("register/provider")]
    public async Task<ActionResult<UserDTO>> RegisterProvider([FromBody] RegisterProviderRequest request)
    {
        var result = await _authService.RegisterProviderAsync(request);
        if (result == null)
            return BadRequest(new { message = "El email ya esta registrado" });

        SetAuthCookie(result.Token);
        return CreatedAtAction(nameof(GetCurrentUser), result.User);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("register/admin")]
    public async Task<ActionResult<UserDTO>> RegisterAdmin([FromBody] RegisterAdminRequest request)
    {
        var user = await _authService.RegisterAdminAsync(request);
        if (user == null)
            return BadRequest(new { message = "El email ya esta registrado" });

        return CreatedAtAction(nameof(GetCurrentUser), user);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(CookieName, new CookieOptions { Path = "/" });
        return NoContent();
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<UserDTO>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _authService.UpdateProfileAsync(userId, request);
        if (user == null) return NotFound();

        return Ok(user);
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

    private void SetAuthCookie(string token)
    {
        Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_env.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(CookieLifetimeDays),
            Path = "/"
        });
    }
}
