using System.Security.Claims;
using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Coworking.WebApi.Controller;

[Authorize(Policy = "ApiPolicy")]
[Route("api/[controller]")]
[ApiController]
public class BookingsController(
    IBookingService bookingService,
    ILogger<BookingsController> logger) : ApiController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            logger.LogWarning("Попытка создания бронирования без аутентификации");
            return Unauthorized();
        }

        request.UserId = Guid.Parse(userIdClaim);
        request.StartTime = request.StartTime.ToUniversalTime();
        request.EndTime = request.EndTime.ToUniversalTime();
        
        logger.LogInformation("API запрос: создание бронирования от пользователя {UserId} для комнаты {RoomId} " +
            "с {StartTime} по {EndTime}", 
            request.UserId, request.RoomId, request.StartTime, request.EndTime);

        var result = await bookingService.CreateBookingAsync(request);
        
        return result.Match(
            booking => {
                logger.LogInformation("Бронирование успешно создано через API: {BookingId}", booking.Id);
                return Created($"api/bookings/{booking.Id}", new { success = true, booking });
            },
            errors => {
                logger.LogWarning("Ошибка при создании бронирования через API: {Errors}", 
                    string.Join("; ", errors.Select(e => e.Description)));
                return Problem(
                    statusCode: 400,
                    title: "Ошибка при создании бронирования",
                    detail: string.Join("; ", errors.Select(e => e.Description))
                );
            }
        );
    }
    
    [HttpGet("my")]
    public async Task<IActionResult> GetMyBookings()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            logger.LogWarning("Попытка получения бронирований без аутентификации");
            return Unauthorized();
        }

        var userId = Guid.Parse(userIdClaim);
        
        logger.LogInformation("API запрос: получение всех бронирований пользователя {UserId}", userId);
        
        var result = await bookingService.GetBookingsByUserAsync(userId);
        
        logger.LogInformation("Найдено {Count} бронирований для пользователя {UserId} через API", 
            result.Count, userId);
        
        return Ok(result);
    }
}

