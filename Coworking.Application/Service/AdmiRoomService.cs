using AutoMapper;
using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Coworking.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ErrorOr;

namespace Coworking.Application.Service;

public class AdminRoomService(
    IApplicationDbContext context,
    IMapper mapper,
    ILogger<AdminRoomService> logger) 
    : IAdminRoomService
{
    public async Task<ErrorOr<RoomDto>> CreateRoomAsync(CreateRoomDto dto)
    {
        logger.LogInformation("Создание новой комнаты: {Name}, емкость: {Capacity}, цена: {Price} ₸/час", 
            dto.Name, dto.Capacity, dto.PricePerHour);
        
        var room = mapper.Map<Room>(dto);
        
        room.PhotoUrl ??= string.Empty; 
        room.PhotoHash ??= string.Empty;
        room.Id = Guid.NewGuid();
        
        room.CreationTime = DateTime.UtcNow;
        room.LastModificationTime = DateTime.UtcNow;
        
        await context.Rooms.AddAsync(room);
        await context.SaveChangesAsync();
        
        logger.LogInformation("Комната успешно создана: {RoomId}, {Name}", room.Id, room.Name);
    
        return mapper.Map<RoomDto>(room);
    }

    public async Task<RoomDto?> UpdatePriceAsync(Guid roomId, decimal price)
    {
        logger.LogInformation("Обновление цены комнаты: {RoomId}, новая цена {Price} ₸/час", roomId, price);
        
        var room = await context.Rooms.FirstOrDefaultAsync(x => x.Id == roomId);
        if (room == null)
        {
            logger.LogWarning("Попытка обновления цены несуществующей комнаты: {RoomId}", roomId);
            return null;
        }

        var oldPrice = room.PricePerHour;
        room.PricePerHour = price;
        room.LastModificationTime = DateTime.UtcNow;
        
        context.Rooms.Update(room);
        await context.SaveChangesAsync();
        
        logger.LogInformation("Цена комнаты успешно обновлена: {RoomId}, было {OldPrice}, стало {NewPrice} ₸/час", 
            roomId, oldPrice, price);
        
        return mapper.Map<RoomDto>(room);
    }

    public async Task<RoomDto?> UpdateRoomAsync(Guid roomId, UpdateRoomDto dto)
    {
        logger.LogInformation("Обновление комнаты: {RoomId}", roomId);
        
        var room = await context.Rooms.FirstOrDefaultAsync(x => x.Id == roomId);

        if (room == null)
        {
            logger.LogError("Комната не найдена при попытке обновления: {RoomId}", roomId);
            throw new Exception("Комната не найдена");
        }
        
        mapper.Map(dto, room);
        room.LastModificationTime = DateTime.UtcNow;

        await context.SaveChangesAsync();
        
        logger.LogInformation("Комната успешно обновлена: {RoomId}, {Name}", roomId, dto.Name);
        
        return mapper.Map<RoomDto>(room);
    }

    public async Task<RoomDto> DeleteRoomAsync(Guid roomId)
    {
        logger.LogInformation("Попытка удаления комнаты: {RoomId}", roomId);
        
        var room = await context.Rooms.FirstOrDefaultAsync(x => x.Id == roomId);
        if (room == null)
        {
            logger.LogError("Ошибка при удалении: комната не найдена {RoomId}", roomId);
            throw new Exception("Комната не найдена");
        }
        
        // Проверяем наличие активных бронирований
        var hasBookings = await context.Bookings
            .AnyAsync(b => b.RoomId == roomId && b.Status != Domain.Enums.BookingStatus.Cancelled);
        
        if (hasBookings)
        {
            logger.LogWarning("Попытка удаления комнаты с активными бронированиями: {RoomId}", roomId);
            throw new Exception("Невозможно удалить комнату с активными бронированиями");
        }
        
        // Удаляем все отмененные бронирования этой комнаты
        var cancelledBookings = await context.Bookings
            .Where(b => b.RoomId == roomId && b.Status == Domain.Enums.BookingStatus.Cancelled)
            .ToListAsync();
        
        foreach (var booking in cancelledBookings)
        {
            context.Bookings.Remove(booking);
        }
        
        context.Rooms.Remove(room);
        await context.SaveChangesAsync();
        
        logger.LogInformation("Комната успешно удалена: {RoomId}, {Name}", roomId, room.Name);
        
        return mapper.Map<RoomDto>(room);
    }
}