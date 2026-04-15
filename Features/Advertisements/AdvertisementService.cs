using Microsoft.EntityFrameworkCore;
using ServiPro.API.Data;
using ServiPro.API.Features.Advertisements.DTOs;
using ServiPro.API.Models.Entities;

namespace ServiPro.API.Features.Advertisements;

public interface IAdvertisementService
{
    Task<List<AdvertisementDTO>> GetActiveByPositionAsync(string? position = null);
    Task<List<AdvertisementDTO>> GetAllAsync();
    Task<AdvertisementDTO?> GetByIdAsync(Guid id);
    Task<AdvertisementDTO> CreateAsync(CreateAdvertisementDTO dto);
    Task<AdvertisementDTO?> UpdateAsync(Guid id, UpdateAdvertisementDTO dto);
    Task<bool> DeleteAsync(Guid id);
    Task RecordImpressionAsync(Guid id);
    Task RecordClickAsync(Guid id);
}

public class AdvertisementService : IAdvertisementService
{
    private readonly ApplicationDbContext _context;

    public AdvertisementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdvertisementDTO>> GetActiveByPositionAsync(string? position = null)
    {
        var now = DateTime.UtcNow;
        var query = _context.Advertisements
            .Where(a => a.Status == "active" && a.StartDate <= now && a.EndDate >= now);

        if (!string.IsNullOrWhiteSpace(position))
            query = query.Where(a => a.Position == position);

        var ads = await query.OrderBy(a => a.CreatedAt).ToListAsync();
        return ads.Select(MapToDTO).ToList();
    }

    public async Task<List<AdvertisementDTO>> GetAllAsync()
    {
        var ads = await _context.Advertisements.OrderByDescending(a => a.CreatedAt).ToListAsync();
        return ads.Select(MapToDTO).ToList();
    }

    public async Task<AdvertisementDTO?> GetByIdAsync(Guid id)
    {
        var ad = await _context.Advertisements.FindAsync(id);
        return ad == null ? null : MapToDTO(ad);
    }

    public async Task<AdvertisementDTO> CreateAsync(CreateAdvertisementDTO dto)
    {
        var ad = new Advertisement
        {
            Title = dto.Title,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            LinkUrl = dto.LinkUrl,
            Position = dto.Position,
            Status = "draft",
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Advertiser = dto.Advertiser
        };

        _context.Advertisements.Add(ad);
        await _context.SaveChangesAsync();
        return MapToDTO(ad);
    }

    public async Task<AdvertisementDTO?> UpdateAsync(Guid id, UpdateAdvertisementDTO dto)
    {
        var ad = await _context.Advertisements.FindAsync(id);
        if (ad == null) return null;

        if (dto.Title != null) ad.Title = dto.Title;
        if (dto.Description != null) ad.Description = dto.Description;
        if (dto.ImageUrl != null) ad.ImageUrl = dto.ImageUrl;
        if (dto.LinkUrl != null) ad.LinkUrl = dto.LinkUrl;
        if (dto.Position != null) ad.Position = dto.Position;
        if (dto.Status != null) ad.Status = dto.Status;
        if (dto.StartDate.HasValue) ad.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) ad.EndDate = dto.EndDate.Value;
        if (dto.Advertiser != null) ad.Advertiser = dto.Advertiser;

        ad.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return MapToDTO(ad);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var ad = await _context.Advertisements.FindAsync(id);
        if (ad == null) return false;

        _context.Advertisements.Remove(ad);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task RecordImpressionAsync(Guid id)
    {
        var ad = await _context.Advertisements.FindAsync(id);
        if (ad == null) return;
        ad.Impressions++;
        await _context.SaveChangesAsync();
    }

    public async Task RecordClickAsync(Guid id)
    {
        var ad = await _context.Advertisements.FindAsync(id);
        if (ad == null) return;
        ad.Clicks++;
        await _context.SaveChangesAsync();
    }

    private static AdvertisementDTO MapToDTO(Advertisement a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        Description = a.Description,
        ImageUrl = a.ImageUrl,
        LinkUrl = a.LinkUrl,
        Position = a.Position,
        Status = a.Status,
        Impressions = a.Impressions,
        Clicks = a.Clicks,
        StartDate = a.StartDate,
        EndDate = a.EndDate,
        Advertiser = a.Advertiser,
        CreatedAt = a.CreatedAt
    };
}
