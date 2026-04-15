using System.ComponentModel.DataAnnotations;
using ServiPro.API.Shared.DTOs;

namespace ServiPro.API.Features.Providers.DTOs;

public class ProviderDTO
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Description { get; set; }
    public string City { get; set; } = string.Empty;
    public string? Zone { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public decimal? HourlyRate { get; set; }
    public bool IsVerified { get; set; }
    public string? Experience { get; set; }
    public string? Availability { get; set; }
    public List<CategoryDTO> Categories { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class ProviderListDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string City { get; set; } = string.Empty;
    public string? Zone { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public decimal? HourlyRate { get; set; }
    public bool IsVerified { get; set; }
    public List<CategoryDTO> Categories { get; set; } = new();
}

public class UpdateProviderRequest
{
    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Zone { get; set; }

    public decimal? HourlyRate { get; set; }

    [MaxLength(100)]
    public string? Experience { get; set; }

    [MaxLength(200)]
    public string? Availability { get; set; }

    public List<int>? CategoryIds { get; set; }
}

public class ProviderSearchRequest
{
    public string? Query { get; set; }
    public int? CategoryId { get; set; }
    public string? City { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
