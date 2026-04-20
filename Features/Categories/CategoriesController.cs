using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiPro.API.Data;
using ServiPro.API.Models.Entities;

namespace ServiPro.API.Features.Categories;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CategoriesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDTO>>> GetAll()
    {
        var categories = await _db.ServiceCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Label)
            .Select(c => new CategoryDTO
            {
                Id = c.Id,
                Code = c.Code,
                Label = c.Label,
                Icon = c.Icon,
                Description = c.Description,
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDTO>> GetById(int id)
    {
        var c = await _db.ServiceCategories.FindAsync(id);
        if (c == null) return NotFound();

        return Ok(new CategoryDTO
        {
            Id = c.Id,
            Code = c.Code,
            Label = c.Label,
            Icon = c.Icon,
            Description = c.Description,
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryDTO>> Create([FromBody] CreateCategoryRequest request)
    {
        if (await _db.ServiceCategories.AnyAsync(c => c.Code == request.Code))
            return BadRequest(new { message = "Ya existe una categoria con ese codigo" });

        var category = new ServiceCategory
        {
            Code = request.Code.ToLower(),
            Label = request.Label,
            Icon = request.Icon,
            Description = request.Description,
            IsActive = true,
        };

        _db.ServiceCategories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, new CategoryDTO
        {
            Id = category.Id,
            Code = category.Code,
            Label = category.Label,
            Icon = category.Icon,
            Description = category.Description,
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDTO>> Update(int id, [FromBody] UpdateCategoryRequest request)
    {
        var category = await _db.ServiceCategories.FindAsync(id);
        if (category == null) return NotFound();

        if (request.Code != null)
        {
            var codeExists = await _db.ServiceCategories.AnyAsync(c => c.Code == request.Code && c.Id != id);
            if (codeExists) return BadRequest(new { message = "Ya existe una categoria con ese codigo" });
            category.Code = request.Code.ToLower();
        }

        if (request.Label != null) category.Label = request.Label;
        if (request.Icon != null) category.Icon = request.Icon;
        if (request.Description != null) category.Description = request.Description;
        if (request.IsActive.HasValue) category.IsActive = request.IsActive.Value;

        await _db.SaveChangesAsync();

        return Ok(new CategoryDTO
        {
            Id = category.Id,
            Code = category.Code,
            Label = category.Label,
            Icon = category.Icon,
            Description = category.Description,
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.ServiceCategories.FindAsync(id);
        if (category == null) return NotFound();

        var hasProviders = await _db.ProviderCategories.AnyAsync(pc => pc.CategoryId == id);
        if (hasProviders)
        {
            category.IsActive = false;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        _db.ServiceCategories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public class CategoryDTO
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateCategoryRequest
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Label { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Icon { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class UpdateCategoryRequest
{
    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(100)]
    public string? Label { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}
