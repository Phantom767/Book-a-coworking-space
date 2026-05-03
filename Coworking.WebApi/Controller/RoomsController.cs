using Coworking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Coworking.WebApi.Controller;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RoomsController(
    IRoomService roomService,
    ILogger<RoomsController> logger) : ControllerBase
{
    // Получить все комнаты
    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        logger.LogInformation("API запрос: получение всех комнат");
        
        var rooms = await roomService.GetAllRoomsAsync();

        logger.LogInformation("Найдено {Count} комнат через API", rooms.Count);

        return Ok(rooms);
    }

    // Получить одну комнату
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoom(Guid id)   
    {
        logger.LogInformation("API запрос: получение комнаты {RoomId}", id);
        
        var rooms = await roomService.GetRoomAsync(id);

        return Ok(rooms);
    }
        
    [HttpGet("{id}/availability")]
    public async Task<IActionResult> BookingBusyTime(Guid id, DateTime date)
    {
        logger.LogInformation("API запрос: получение занятого времени для комнаты {RoomId} на дату {Date}", 
            id, date.Date);
        
        var result = await roomService.BookingBusyTimeAsync(id, date);

        logger.LogInformation("Получены данные о занятости для комнаты {RoomId}", id);

        return Ok(result);
    }
}