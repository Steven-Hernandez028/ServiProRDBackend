using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiPro.API.Features.Reviews.DTOs;

namespace ServiPro.API.Features.Reviews;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("provider/{providerId:guid}")]
    public async Task<ActionResult<List<ReviewDTO>>> GetProviderReviews(Guid providerId)
    {
        var reviews = await _reviewService.GetReviewsByProviderIdAsync(providerId);
        return Ok(reviews);
    }

    [Authorize(Roles = "Client")]
    [HttpPost]
    public async Task<ActionResult<ReviewDTO>> CreateReview([FromBody] CreateReviewDTO dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var review = await _reviewService.CreateReviewAsync(userId.Value, dto);
        if (review == null)
            return BadRequest(new { message = "No se pudo crear la resena" });

        return CreatedAtAction(nameof(GetProviderReviews), new { providerId = review.ProviderId }, review);
    }

    [Authorize(Roles = "Provider")]
    [HttpPost("{id:guid}/response")]
    public async Task<ActionResult> AddProviderResponse(Guid id, [FromBody] ProviderResponseDTO dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var success = await _reviewService.AddProviderResponseAsync(id, userId.Value, dto.Response);
        if (!success)
            return BadRequest(new { message = "No se pudo agregar la respuesta" });

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<ActionResult<List<ReviewDTO>>> GetAllReviews()
    {
        var reviews = await _reviewService.GetAllReviewsAsync();
        return Ok(reviews);
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
