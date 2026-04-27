using System.Security.Claims;
using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Coworking.WebApi.Pages.Rooms;

public class Details(IRoomService roomService, IBookingService bookingService) : PageModel
{
    [BindProperty]
    public BookingRequest Input { get; set; }
    public RoomDto Room { get; set; } = null!;

    public async Task OnGetAsync(Guid id)
    {
        Room = await roomService.GetRoomAsync(id);
    }
    
    public class BookingRequest
    {
        public Guid RoomId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public async Task<IActionResult> OnPostCreateAsync([FromBody] BookingRequest request)
    {
        // 1. Проверка авторизации
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return new JsonResult(new { success = false, message = "Unauthorized" });

        // 2. Подготовка данных
        var dto = new CreateBookingDto
        {
            UserId = Guid.Parse(userIdClaim),
            RoomId = request.RoomId,
            StartTime = request.StartTime.ToUniversalTime(),
            EndTime = request.EndTime.ToUniversalTime()
        };

        var result = await bookingService.CreateBookingAsync(dto);

        return result.Match(
            booking => new JsonResult(new { success = true, bookingId = booking.Id }),
            errors => new JsonResult(new { 
                success = false, 
                message = string.Join("; ", errors.Select(e => e.Description)) 
            })
        );
    }
}