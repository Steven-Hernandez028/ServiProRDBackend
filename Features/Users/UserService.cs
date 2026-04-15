using Microsoft.EntityFrameworkCore;
using ServiPro.API.Data;
using ServiPro.API.Features.Users.DTOs;
using ServiPro.API.Models.Entities;

namespace ServiPro.API.Features.Users;

public interface IUserService
{
    Task<List<ClientDTO>> GetAllClientsAsync();
    Task<List<UserListDTO>> GetAllUsersAsync();
    Task<bool> ToggleUserActiveAsync(Guid userId);
}

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClientDTO>> GetAllClientsAsync()
    {
        var clients = await _context.Clients
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return clients.Select(c => new ClientDTO
        {
            Id = c.Id,
            UserId = c.UserId,
            Email = c.User.Email,
            Name = c.User.Name,
            Phone = c.User.Phone,
            City = c.City,
            Address = c.Address,
            IsActive = c.User.IsActive,
            CreatedAt = c.User.CreatedAt
        }).ToList();
    }

    public async Task<List<UserListDTO>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return users.Select(u => new UserListDTO
        {
            Id = u.Id,
            Email = u.Email,
            Name = u.Name,
            Phone = u.Phone,
            Role = u.Role.ToString(),
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        }).ToList();
    }

    public async Task<bool> ToggleUserActiveAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();
        return true;
    }
}
