using Coworking.WebApi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RoomsController(IRoomService roomService) : ControllerBase
{
    // Получить все комнаты
    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await roomService.GetAllRoomsAsync();

        return Ok(rooms);
    }

    // Получить одну комнату
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoom(Guid id)   
    {
        var rooms = await roomService.GetRoomAsync(id);

        return Ok(rooms);
    }
        
    [HttpGet("{id}/availability")]
    public async Task<IActionResult> BookingBusyTime(Guid id, DateTime date)
    {        
        var result = await roomService.BookingBusyTimeAsync(id, date);

        return Ok(result);
    }
}