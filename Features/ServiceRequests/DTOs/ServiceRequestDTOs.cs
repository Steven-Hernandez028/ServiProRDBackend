using System.ComponentModel.DataAnnotations;
using ServiPro.API.Models.Entities;
using ServiPro.API.Shared.DTOs;

namespace ServiPro.API.Features.ServiceRequests.DTOs;

public class ServiceRequestDTO
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? ClientPhone { get; set; }
    public Guid? ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public CategoryDTO? Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Zone { get; set; }
    public string Address { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
    public decimal? Budget { get; set; }
    public DateOnly? PreferredDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateServiceRequestDTO
{
    [Required]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Zone { get; set; }

    [Required]
    public string Address { get; set; } = string.Empty;

    public decimal? Budget { get; set; }

    public DateOnly? PreferredDate { get; set; }

    public Guid? ProviderId { get; set; }
}

public class UpdateServiceRequestStatusDTO
{
    [Required]
    public RequestStatus Status { get; set; }
}

public class ServiceRequestSearchDTO
{
    public int? CategoryId { get; set; }
    public string? City { get; set; }
    public RequestStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
