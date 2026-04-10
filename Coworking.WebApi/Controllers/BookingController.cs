using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.WebApi.Controllers;

public class BookingsController(IBookingService bookingService) : ApiController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingDto request)
    {
        var result = await bookingService.CreateBookingAsync(request);

        // Используем метод Match из ErrorOr: 
        // Если успех (Value) -> возвращаем 201 Created
        // Если ошибка (Errors) -> вызываем наш метод Problem
        return result.Match(
            booking => CreatedAtAction(nameof(Create), new { id = booking.Id }, booking),
            Problem
        );
    }
}