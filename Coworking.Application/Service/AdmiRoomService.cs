using AutoMapper;
using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Coworking.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using ErrorOr;

namespace Coworking.Application.Service;

public class AdminRoomService(
    IApplicationDbContext context,
    IMapper mapper) 
    : IAdminRoomService
{
    public async Task<ErrorOr<RoomDto>> CreateRoomAsync(CreateRoomDto dto)
    {
        var room = mapper.Map<Room>(dto);
        
        room.PhotoUrl ??= string.Empty; 
        room.PhotoHash ??= string.Empty;
        
        room.CreationTime = DateTime.UtcNow;
        room.LastModificationTime = DateTime.UtcNow;
        
        await context.Rooms.AddAsync(room);
        await context.SaveChangesAsync();
    
        return mapper.Map<RoomDto>(room);
    }

    public async Task<RoomDto?> UpdatePriceAsync(Guid roomId, decimal price)
    {
        var room = await context.Rooms.FirstOrDefaultAsync(x => x.Id == roomId);
        if (room == null)
            return null;

        room.PricePerHour = price;
        context.Rooms.Update(room);
        await context.SaveChangesAsync();
        
        return mapper.Map<RoomDto>(room);
    }

    public async Task<RoomDto> DeleteRoomAsync(Guid roomId)
    {
        var room = context.Rooms.FirstOrDefault(x => x.Id == roomId);
        if (room == null)
        {
            throw new Exception("Комната не найдена");
        }
        
        context.Rooms.Remove(room);
        await context.SaveChangesAsync();
        
        return mapper.Map<RoomDto>(room);
    }
}