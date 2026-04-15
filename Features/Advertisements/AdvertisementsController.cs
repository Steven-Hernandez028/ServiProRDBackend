using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiPro.API.Features.Advertisements.DTOs;

namespace ServiPro.API.Features.Advertisements;

[ApiController]
[Route("api/advertisements")]
public class AdvertisementsController : ControllerBase
{
    private readonly IAdvertisementService _adService;

    public AdvertisementsController(IAdvertisementService adService)
    {
        _adService = adService;
    }

    /// <summary>Public: get active ads, optionally filtered by position</summary>
    [HttpGet]
    public async Task<ActionResult<List<AdvertisementDTO>>> GetActive([FromQuery] string? position = null)
    {
        var ads = await _adService.GetActiveByPositionAsync(position);
        return Ok(ads);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<ActionResult<List<AdvertisementDTO>>> GetAll()
    {
        var ads = await _adService.GetAllAsync();
        return Ok(ads);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdvertisementDTO>> GetById(Guid id)
    {
        var ad = await _adService.GetByIdAsync(id);
        if (ad == null) return NotFound();
        return Ok(ad);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<AdvertisementDTO>> Create([FromBody] CreateAdvertisementDTO dto)
    {
        var ad = await _adService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = ad.Id }, ad);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdvertisementDTO>> Update(Guid id, [FromBody] UpdateAdvertisementDTO dto)
    {
        var ad = await _adService.UpdateAsync(id, dto);
        if (ad == null) return NotFound();
        return Ok(ad);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _adService.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:guid}/impression")]
    public async Task<ActionResult> RecordImpression(Guid id)
    {
        await _adService.RecordImpressionAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/click")]
    public async Task<ActionResult> RecordClick(Guid id)
    {
        await _adService.RecordClickAsync(id);
        return NoContent();
    }
}
