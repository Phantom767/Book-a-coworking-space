using System.Security.Claims;
using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Coworking.WebApi.Pages.Bookings;

public class MyBookingsModel(IBookingService bookingService) : PageModel
{
    public IEnumerable<BookingDto> Bookings { get; private set; } = new List<BookingDto>();

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            return RedirectToPage("/Account/Login"); // В Razor Pages лучше редиректить на логин
        }

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest("Invalid User ID");
        }

        var result = await bookingService.GetBookingsByUserAsync(userId);
        // if (result.IsError)
        // {
        //     return StatusCode(500, "Ошибка при загрузке бронирований");
        // }
        Bookings = result;
        return Page();    
    }
}