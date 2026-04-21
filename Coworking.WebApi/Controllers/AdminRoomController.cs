using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.WebApi.Controllers;

[ApiController]
[Route("api/[controller]/rooms")]
[Authorize(Roles = "Admin")]
public class AdminRoomsController(IAdminRoomService roomService) 
    : ApiController
{
    // Создать комнату
    [HttpPost]
    public async Task<IActionResult> CreateRoom(CreateRoomDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        var room = await roomService.CreateRoomAsync(dto);

        return room.Match(
            value => CreatedAtAction(nameof(CreateRoom), new { id = value.Id }, value),
            Problem
        );
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
}