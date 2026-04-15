using Microsoft.EntityFrameworkCore;
using ServiPro.API.Data;
using ServiPro.API.Features.SocialMedia.DTOs;
using ServiPro.API.Models.Entities;

namespace ServiPro.API.Features.SocialMedia;

public interface ISocialMediaService
{
    Task<List<SocialMediaLinkDTO>> GetActiveLinksAsync();
    Task<List<SocialMediaLinkDTO>> GetAllLinksAsync();
    Task<SocialMediaLinkDTO?> GetByIdAsync(Guid id);
    Task<SocialMediaLinkDTO> CreateAsync(CreateSocialMediaLinkDTO dto);
    Task<SocialMediaLinkDTO?> UpdateAsync(Guid id, UpdateSocialMediaLinkDTO dto);
    Task<bool> DeleteAsync(Guid id);
}

public class SocialMediaService : ISocialMediaService
{
    private readonly ApplicationDbContext _context;

    public SocialMediaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SocialMediaLinkDTO>> GetActiveLinksAsync()
    {
        var links = await _context.SocialMediaLinks
            .Where(s => s.IsActive)
            .OrderBy(s => s.Order)
            .ToListAsync();

        return links.Select(MapToDTO).ToList();
    }

    public async Task<List<SocialMediaLinkDTO>> GetAllLinksAsync()
    {
        var links = await _context.SocialMediaLinks
            .OrderBy(s => s.Order)
            .ToListAsync();

        return links.Select(MapToDTO).ToList();
    }

    public async Task<SocialMediaLinkDTO?> GetByIdAsync(Guid id)
    {
        var link = await _context.SocialMediaLinks.FindAsync(id);
        return link == null ? null : MapToDTO(link);
    }

    public async Task<SocialMediaLinkDTO> CreateAsync(CreateSocialMediaLinkDTO dto)
    {
        var link = new SocialMediaLink
        {
            Platform = dto.Platform,
            Url = dto.Url,
            Label = dto.Label,
            IsActive = dto.IsActive,
            Order = dto.Order,
            Followers = dto.Followers
        };

        _context.SocialMediaLinks.Add(link);
        await _context.SaveChangesAsync();
        return MapToDTO(link);
    }

    public async Task<SocialMediaLinkDTO?> UpdateAsync(Guid id, UpdateSocialMediaLinkDTO dto)
    {
        var link = await _context.SocialMediaLinks.FindAsync(id);
        if (link == null) return null;

        if (dto.Url != null) link.Url = dto.Url;
        if (dto.Label != null) link.Label = dto.Label;
        if (dto.IsActive.HasValue) link.IsActive = dto.IsActive.Value;
        if (dto.Order.HasValue) link.Order = dto.Order.Value;
        if (dto.Followers != null) link.Followers = dto.Followers;

        link.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return MapToDTO(link);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var link = await _context.SocialMediaLinks.FindAsync(id);
        if (link == null) return false;

        _context.SocialMediaLinks.Remove(link);
        await _context.SaveChangesAsync();
        return true;
    }

    private static SocialMediaLinkDTO MapToDTO(SocialMediaLink s) => new()
    {
        Id = s.Id,
        Platform = s.Platform,
        Url = s.Url,
        Label = s.Label,
        IsActive = s.IsActive,
        Order = s.Order,
        Followers = s.Followers,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
