using Microsoft.EntityFrameworkCore;
using ServiPro.API.Data;
using ServiPro.API.Features.Providers.DTOs;
using ServiPro.API.Models.Entities;
using ServiPro.API.Shared.DTOs;

namespace ServiPro.API.Features.Providers;

public interface IProviderService
{
    Task<PagedResult<ProviderListDTO>> SearchProvidersAsync(ProviderSearchRequest request);
    Task<ProviderDTO?> GetProviderByIdAsync(Guid id);
    Task<ProviderDTO?> GetProviderByUserIdAsync(Guid userId);
    Task<ProviderDTO?> UpdateProviderAsync(Guid userId, UpdateProviderRequest request);
    Task<bool> VerifyProviderAsync(Guid providerId);
    Task<bool> ToggleProviderActiveAsync(Guid providerId);
    Task<List<ProviderDTO>> GetAllProvidersAsync();
}

public class ProviderService : IProviderService
{
    private readonly ApplicationDbContext _context;

    public ProviderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProviderListDTO>> SearchProvidersAsync(ProviderSearchRequest request)
    {
        var query = _context.Providers
            .Include(p => p.User)
            .Include(p => p.Categories).ThenInclude(c => c.Category)
            .Where(p => p.User.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var term = request.Query.ToLower();
            query = query.Where(p =>
                p.User.Name.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.Categories.Any(c => c.CategoryId == request.CategoryId.Value));

        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(p => p.City == request.City);

        var totalCount = await query.CountAsync();

        var providers = await query
            .OrderByDescending(p => p.Rating)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<ProviderListDTO>
        {
            Items = providers.Select(MapToListDTO).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<ProviderDTO?> GetProviderByIdAsync(Guid id)
    {
        var provider = await _context.Providers
            .Include(p => p.User)
            .Include(p => p.Categories).ThenInclude(c => c.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        return provider == null ? null : MapToDTO(provider);
    }

    public async Task<ProviderDTO?> GetProviderByUserIdAsync(Guid userId)
    {
        var provider = await _context.Providers
            .Include(p => p.User)
            .Include(p => p.Categories).ThenInclude(c => c.Category)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        return provider == null ? null : MapToDTO(provider);
    }

    public async Task<ProviderDTO?> UpdateProviderAsync(Guid userId, UpdateProviderRequest request)
    {
        var provider = await _context.Providers
            .Include(p => p.User)
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (provider == null) return null;

        if (request.Description != null) provider.Description = request.Description;
        if (request.City != null) provider.City = request.City;
        if (request.Zone != null) provider.Zone = request.Zone;
        if (request.HourlyRate.HasValue) provider.HourlyRate = request.HourlyRate;
        if (request.Experience != null) provider.Experience = request.Experience;
        if (request.Availability != null) provider.Availability = request.Availability;

        if (request.CategoryIds != null)
        {
            _context.ProviderCategories.RemoveRange(provider.Categories);
            foreach (var categoryId in request.CategoryIds)
            {
                _context.ProviderCategories.Add(new ProviderCategory
                {
                    ProviderId = provider.Id,
                    CategoryId = categoryId
                });
            }
        }

        provider.UpdatedAt = DateTime.UtcNow;
        provider.User.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetProviderByIdAsync(provider.Id);
    }

    public async Task<bool> VerifyProviderAsync(Guid providerId)
    {
        var provider = await _context.Providers.FindAsync(providerId);
        if (provider == null) return false;

        provider.IsVerified = true;
        provider.VerifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleProviderActiveAsync(Guid providerId)
    {
        var provider = await _context.Providers
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == providerId);

        if (provider == null) return false;

        provider.User.IsActive = !provider.User.IsActive;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ProviderDTO>> GetAllProvidersAsync()
    {
        var providers = await _context.Providers
            .Include(p => p.User)
            .Include(p => p.Categories).ThenInclude(c => c.Category)
            .ToListAsync();

        return providers.Select(MapToDTO).ToList();
    }

    private static ProviderDTO MapToDTO(Provider p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        Email = p.User.Email,
        Name = p.User.Name,
        Phone = p.User.Phone,
        Description = p.Description,
        City = p.City,
        Zone = p.Zone,
        Rating = p.Rating,
        ReviewCount = p.ReviewCount,
        HourlyRate = p.HourlyRate,
        IsVerified = p.IsVerified,
        Experience = p.Experience,
        Availability = p.Availability,
        Categories = p.Categories.Select(c => new CategoryDTO
        {
            Id = c.Category.Id,
            Code = c.Category.Code,
            Label = c.Category.Label,
            Icon = c.Category.Icon
        }).ToList(),
        CreatedAt = p.User.CreatedAt,
        IsActive = p.User.IsActive
    };

    private static ProviderListDTO MapToListDTO(Provider p) => new()
    {
        Id = p.Id,
        Name = p.User.Name,
        Description = p.Description,
        City = p.City,
        Zone = p.Zone,
        Rating = p.Rating,
        ReviewCount = p.ReviewCount,
        HourlyRate = p.HourlyRate,
        IsVerified = p.IsVerified,
        Categories = p.Categories.Select(c => new CategoryDTO
        {
            Id = c.Category.Id,
            Code = c.Category.Code,
            Label = c.Category.Label,
            Icon = c.Category.Icon
        }).ToList()
    };
}
