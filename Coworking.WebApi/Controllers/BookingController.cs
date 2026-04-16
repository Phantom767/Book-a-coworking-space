using System.Security.Claims;
using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.WebApi.Controllers;

[Authorize]
[Route("api/[controller]")]
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
        // Используем метод Match из ErrorOr: 
        // Если успех (Value) -> возвращаем 201 Created
        // Если ошибка (Errors) -> вызываем наш метод Problem
        return result.Match(
            booking => CreatedAtAction(nameof(Create), new { id = booking.Id }, booking),
            Problem
        );
    }
    
    [HttpGet("my")]
    public async Task<IActionResult> GetMyBookings()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return Unauthorized();

        var userId = Guid.Parse(userIdClaim);
        var result = await bookingService.GetBookingsByUserAsync(userId);
        
        return result.Match(
            bookings => Ok(bookings),
            Problem
        );
    }
}