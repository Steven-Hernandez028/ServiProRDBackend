using Microsoft.EntityFrameworkCore;
using ServiPro.API.Data;
using ServiPro.API.Features.Reviews.DTOs;
using ServiPro.API.Models.Entities;

namespace ServiPro.API.Features.Reviews;

public interface IReviewService
{
    Task<List<ReviewDTO>> GetReviewsByProviderIdAsync(Guid providerId);
    Task<ReviewDTO?> CreateReviewAsync(Guid clientUserId, CreateReviewDTO dto);
    Task<bool> AddProviderResponseAsync(Guid reviewId, Guid providerUserId, string response);
    Task<List<ReviewDTO>> GetAllReviewsAsync();
}

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;

    public ReviewService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReviewDTO>> GetReviewsByProviderIdAsync(Guid providerId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.Client).ThenInclude(c => c.User)
            .Where(r => r.ProviderId == providerId && r.IsVisible)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(MapToDTO).ToList();
    }

    public async Task<ReviewDTO?> CreateReviewAsync(Guid clientUserId, CreateReviewDTO dto)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (client == null) return null;

        // Prevent duplicate review for same provider by same client
        var existing = await _context.Reviews
            .AnyAsync(r => r.ProviderId == dto.ProviderId && r.ClientId == client.Id &&
                           (dto.ServiceRequestId == null || r.ServiceRequestId == dto.ServiceRequestId));
        if (existing) return null;

        var review = new Review
        {
            ProviderId = dto.ProviderId,
            ClientId = client.Id,
            ServiceRequestId = dto.ServiceRequestId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };

        _context.Reviews.Add(review);

        // Update provider rating
        await _context.SaveChangesAsync();
        await UpdateProviderRatingAsync(dto.ProviderId);

        var saved = await _context.Reviews
            .Include(r => r.Client).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(r => r.Id == review.Id);

        return saved == null ? null : MapToDTO(saved);
    }

    public async Task<bool> AddProviderResponseAsync(Guid reviewId, Guid providerUserId, string response)
    {
        var review = await _context.Reviews
            .Include(r => r.Provider).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null || review.Provider.UserId != providerUserId) return false;

        review.ProviderResponse = response;
        review.ProviderResponseAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ReviewDTO>> GetAllReviewsAsync()
    {
        var reviews = await _context.Reviews
            .Include(r => r.Client).ThenInclude(c => c.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(MapToDTO).ToList();
    }

    private async Task UpdateProviderRatingAsync(Guid providerId)
    {
        var provider = await _context.Providers.FindAsync(providerId);
        if (provider == null) return;

        var stats = await _context.Reviews
            .Where(r => r.ProviderId == providerId && r.IsVisible)
            .GroupBy(r => r.ProviderId)
            .Select(g => new { Avg = g.Average(r => r.Rating), Count = g.Count() })
            .FirstOrDefaultAsync();

        if (stats != null)
        {
            provider.Rating = (decimal)stats.Avg;
            provider.ReviewCount = stats.Count;
            await _context.SaveChangesAsync();
        }
    }

    private static ReviewDTO MapToDTO(Review r) => new()
    {
        Id = r.Id,
        ProviderId = r.ProviderId,
        ClientId = r.ClientId,
        ClientName = r.Client.User.Name,
        Rating = r.Rating,
        Comment = r.Comment,
        ProviderResponse = r.ProviderResponse,
        CreatedAt = r.CreatedAt
    };
}
