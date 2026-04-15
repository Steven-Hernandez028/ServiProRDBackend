using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiPro.API.Features.Users.DTOs;

namespace ServiPro.API.Features.Users;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("clients")]
    public async Task<ActionResult<List<ClientDTO>>> GetAllClients()
    {
        var clients = await _userService.GetAllClientsAsync();
        return Ok(clients);
    }

    [HttpGet]
    public async Task<ActionResult<List<UserListDTO>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPost("{id:guid}/toggle-active")]
    public async Task<ActionResult> ToggleUserActive(Guid id)
    {
        var success = await _userService.ToggleUserActiveAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
