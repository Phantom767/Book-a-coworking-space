using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.WebApi.Controller;

[ApiController]
[Route("api/[controller]/rooms")]
[Authorize(Roles = "Admin")]
public class AdminRoomsController(
    IAdminRoomService roomService,
    ILogger<AdminRoomsController> logger) 
    : ApiController
{
    // Создать комнату
    [HttpPost]
    public async Task<IActionResult> CreateRoom(CreateRoomDto dto)
    {
        try
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            
            logger.LogInformation("API запрос: создание комнаты {Name}", dto.Name);
            
            var room = await roomService.CreateRoomAsync(dto);

            return room.Match(
                value => CreatedAtAction(nameof(CreateRoom), new { id = value.Id }, value),
                Problem
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при создании комнаты");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // Обновить цену
    [HttpPut("{id}/price")]
    public async Task<IActionResult> UpdatePrice(Guid id, decimal price)
    {
        try
        {
            logger.LogInformation("API запрос: обновление цены комнаты {RoomId}", id);
            
            var room = await roomService.UpdatePriceAsync(id, price);
            
            if (room == null)
            {
                logger.LogWarning("Комната не найдена при обновлении цены: {RoomId}", id);
                return NotFound(new { success = false, message = "Комната не найдена" });
            }

            return Ok(new { success = true, data = room });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обновлении цены комнаты {RoomId}", id);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // Удалить
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(Guid id)
    {
        try
        {
            logger.LogInformation("API запрос: удаление комнаты {RoomId}", id);
            
            var room = await roomService.DeleteRoomAsync(id);

            logger.LogInformation("Комната успешно удалена через API: {RoomId}", id);
            return Ok(new { success = true, data = room, message = "Комната успешно удалена" });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Ошибка при удалении комнаты {RoomId}: {Message}", id, ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}