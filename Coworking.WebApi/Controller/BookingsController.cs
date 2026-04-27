using System.Security.Claims;
using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.WebApi.Controller;

[Authorize(Policy = "ApiPolicy")]
[Route("api/[controller]")]
[ApiController]
public class BookingsController(IBookingService bookingService) : ApiController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return Unauthorized();

        request.UserId = Guid.Parse(userIdClaim);
        request.StartTime = request.StartTime.ToUniversalTime();
        request.EndTime = request.EndTime.ToUniversalTime();

        var result = await bookingService.CreateBookingAsync(request);
        
        return result.Match(
            booking => Created($"api/bookings/{booking.Id}", new { success = true, booking }),
            errors => Problem(
                statusCode: 400,
                title: "Ошибка при создании бронирования",
                detail: string.Join("; ", errors.Select(e => e.Description))
            )
        );
    }
    
    [HttpGet("my")]
    public async Task<IActionResult> GetMyBookings()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return Unauthorized();

        var userId = Guid.Parse(userIdClaim);
        var result = await bookingService.GetBookingsByUserAsync(userId);
        
        return Ok(result);
    }
}

