using Coworking.Application.DTOs;
using Coworking.WebApi.Service;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.WebApi.Controllers;

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

    // Создать комнату
    [HttpPost]
    public async Task<IActionResult> CreateRoom(CreateRoomDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        var rooms = await roomService.CreateRoomAsync(dto);

        return Ok(rooms);
    }

    // Обновить цену
    [HttpPut("{id}/price")]
    public async Task<IActionResult> UpdatePrice(Guid id, decimal price)
    {
        var rooms = await roomService.UpdatePriceAsync(id, price);

        return Ok(rooms);
    }

    // Удалить
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(Guid id)
    {
        var rooms = await roomService.DeleteRoomAsync(id);

        return Ok(rooms);
    }
    
    [HttpGet("{id}/availability")]
    public async Task<IActionResult> BookingBusyTime(Guid id, DateTime date)
    {        
        var result = await roomService.BookingBusyTimeAsync(id, date);

        return Ok(result);
    }
}