using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiPro.API.Features.SocialMedia.DTOs;

namespace ServiPro.API.Features.SocialMedia;

[ApiController]
[Route("api/social-media")]
public class SocialMediaController : ControllerBase
{
    private readonly ISocialMediaService _service;

    public SocialMediaController(ISocialMediaService service)
    {
        _service = service;
    }

    /// <summary>Public: get active social media links</summary>
    [HttpGet]
    public async Task<ActionResult<List<SocialMediaLinkDTO>>> GetActive()
    {
        var links = await _service.GetActiveLinksAsync();
        return Ok(links);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<ActionResult<List<SocialMediaLinkDTO>>> GetAll()
    {
        var links = await _service.GetAllLinksAsync();
        return Ok(links);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SocialMediaLinkDTO>> GetById(Guid id)
    {
        var link = await _service.GetByIdAsync(id);
        if (link == null) return NotFound();
        return Ok(link);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<SocialMediaLinkDTO>> Create([FromBody] CreateSocialMediaLinkDTO dto)
    {
        var link = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = link.Id }, link);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SocialMediaLinkDTO>> Update(Guid id, [FromBody] UpdateSocialMediaLinkDTO dto)
    {
        var link = await _service.UpdateAsync(id, dto);
        if (link == null) return NotFound();
        return Ok(link);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
