using Microsoft.EntityFrameworkCore;
using ServiPro.API.Data;
using ServiPro.API.Features.ServiceRequests.DTOs;
using ServiPro.API.Models.Entities;
using ServiPro.API.Shared.DTOs;

namespace ServiPro.API.Features.ServiceRequests;

public interface IServiceRequestService
{
    Task<ServiceRequestDTO?> CreateRequestAsync(Guid clientUserId, CreateServiceRequestDTO request);
    Task<ServiceRequestDTO?> GetRequestByIdAsync(Guid id);
    Task<PagedResult<ServiceRequestDTO>> GetRequestsForClientAsync(Guid clientUserId, ServiceRequestSearchDTO search);
    Task<PagedResult<ServiceRequestDTO>> GetRequestsForProviderAsync(Guid providerUserId, ServiceRequestSearchDTO search);
    Task<PagedResult<ServiceRequestDTO>> GetAvailableRequestsAsync(Guid providerUserId, ServiceRequestSearchDTO search);
    Task<PagedResult<ServiceRequestDTO>> GetAllRequestsAsync(ServiceRequestSearchDTO search);
    Task<bool> AcceptRequestAsync(Guid requestId, Guid providerUserId);
    Task<bool> UpdateRequestStatusAsync(Guid requestId, Guid userId, RequestStatus status);
}

public class ServiceRequestService : IServiceRequestService
{
    private readonly ApplicationDbContext _context;

    public ServiceRequestService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceRequestDTO?> CreateRequestAsync(Guid clientUserId, CreateServiceRequestDTO request)
    {
        var client = await _context.Clients
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == clientUserId);

        if (client == null) return null;

        var serviceRequest = new ServiceRequest
        {
            ClientId = client.Id,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            City = request.City,
            Zone = request.Zone,
            Address = request.Address,
            Budget = request.Budget,
            PreferredDate = request.PreferredDate,
            ProviderId = request.ProviderId
        };

        _context.ServiceRequests.Add(serviceRequest);
        await _context.SaveChangesAsync();

        return await GetRequestByIdAsync(serviceRequest.Id);
    }

    public async Task<ServiceRequestDTO?> GetRequestByIdAsync(Guid id)
    {
        var request = await _context.ServiceRequests
            .Include(sr => sr.Client).ThenInclude(c => c.User)
            .Include(sr => sr.Provider).ThenInclude(p => p!.User)
            .Include(sr => sr.Category)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        return request == null ? null : MapToDTO(request);
    }

    public async Task<PagedResult<ServiceRequestDTO>> GetRequestsForClientAsync(Guid clientUserId, ServiceRequestSearchDTO search)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (client == null) return new PagedResult<ServiceRequestDTO>();

        var query = _context.ServiceRequests
            .Include(sr => sr.Client).ThenInclude(c => c.User)
            .Include(sr => sr.Provider).ThenInclude(p => p!.User)
            .Include(sr => sr.Category)
            .Where(sr => sr.ClientId == client.Id);

        return await ExecuteSearchAsync(query, search);
    }

    public async Task<PagedResult<ServiceRequestDTO>> GetRequestsForProviderAsync(Guid providerUserId, ServiceRequestSearchDTO search)
    {
        var provider = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == providerUserId);
        if (provider == null) return new PagedResult<ServiceRequestDTO>();

        var query = _context.ServiceRequests
            .Include(sr => sr.Client).ThenInclude(c => c.User)
            .Include(sr => sr.Provider).ThenInclude(p => p!.User)
            .Include(sr => sr.Category)
            .Where(sr => sr.ProviderId == provider.Id);

        return await ExecuteSearchAsync(query, search);
    }

    public async Task<PagedResult<ServiceRequestDTO>> GetAvailableRequestsAsync(Guid providerUserId, ServiceRequestSearchDTO search)
    {
        var provider = await _context.Providers
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.UserId == providerUserId);

        if (provider == null) return new PagedResult<ServiceRequestDTO>();

        var providerCategoryIds = provider.Categories.Select(c => c.CategoryId).ToList();

        var query = _context.ServiceRequests
            .Include(sr => sr.Client).ThenInclude(c => c.User)
            .Include(sr => sr.Provider).ThenInclude(p => p!.User)
            .Include(sr => sr.Category)
            .Where(sr => sr.ProviderId == null &&
                         sr.Status == RequestStatus.Pending &&
                         sr.City == provider.City &&
                         providerCategoryIds.Contains(sr.CategoryId));

        return await ExecuteSearchAsync(query, search);
    }

    public async Task<PagedResult<ServiceRequestDTO>> GetAllRequestsAsync(ServiceRequestSearchDTO search)
    {
        var query = _context.ServiceRequests
            .Include(sr => sr.Client).ThenInclude(c => c.User)
            .Include(sr => sr.Provider).ThenInclude(p => p!.User)
            .Include(sr => sr.Category);

        return await ExecuteSearchAsync(query, search);
    }

    public async Task<bool> AcceptRequestAsync(Guid requestId, Guid providerUserId)
    {
        var provider = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == providerUserId);
        if (provider == null) return false;

        var request = await _context.ServiceRequests.FindAsync(requestId);
        if (request == null || request.ProviderId != null) return false;

        request.ProviderId = provider.Id;
        request.Status = RequestStatus.Accepted;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateRequestStatusAsync(Guid requestId, Guid userId, RequestStatus status)
    {
        var request = await _context.ServiceRequests
            .Include(sr => sr.Provider)
            .Include(sr => sr.Client)
            .FirstOrDefaultAsync(sr => sr.Id == requestId);

        if (request == null) return false;

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        if (user.Role == UserRole.Client && request.Client.UserId != userId) return false;
        if (user.Role == UserRole.Provider && request.Provider?.UserId != userId) return false;

        request.Status = status;
        request.UpdatedAt = DateTime.UtcNow;

        if (status == RequestStatus.Completed) request.CompletedAt = DateTime.UtcNow;
        if (status == RequestStatus.Cancelled) request.CancelledAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<PagedResult<ServiceRequestDTO>> ExecuteSearchAsync(
        IQueryable<ServiceRequest> query, ServiceRequestSearchDTO search)
    {
        if (search.CategoryId.HasValue)
            query = query.Where(sr => sr.CategoryId == search.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(search.City))
            query = query.Where(sr => sr.City == search.City);

        if (search.Status.HasValue)
            query = query.Where(sr => sr.Status == search.Status.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(sr => sr.CreatedAt)
            .Skip((search.Page - 1) * search.PageSize)
            .Take(search.PageSize)
            .ToListAsync();

        return new PagedResult<ServiceRequestDTO>
        {
            Items = items.Select(MapToDTO).ToList(),
            TotalCount = totalCount,
            Page = search.Page,
            PageSize = search.PageSize
        };
    }

    private static ServiceRequestDTO MapToDTO(ServiceRequest sr) => new()
    {
        Id = sr.Id,
        ClientId = sr.ClientId,
        ClientName = sr.Client.User.Name,
        ClientPhone = sr.Client.User.Phone,
        ProviderId = sr.ProviderId,
        ProviderName = sr.Provider?.User.Name,
        Category = sr.Category == null ? null : new CategoryDTO
        {
            Id = sr.Category.Id,
            Code = sr.Category.Code,
            Label = sr.Category.Label,
            Icon = sr.Category.Icon
        },
        Title = sr.Title,
        Description = sr.Description,
        City = sr.City,
        Zone = sr.Zone,
        Address = sr.Address,
        Status = sr.Status,
        Budget = sr.Budget,
        PreferredDate = sr.PreferredDate,
        CreatedAt = sr.CreatedAt,
        UpdatedAt = sr.UpdatedAt
    };
}
