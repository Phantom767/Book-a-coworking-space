using Coworking.Application.DTOs;
using Coworking.Infrastructure.Persistence;
using AutoMapper;
using Coworking.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Coworking.WebApi.Service;

public class RoomService(ApplicationDbContext context, IMapper mapper) : IRoomService
{
    public async Task<List<RoomDto>> GetAllRoomsAsync()
    {
        var rooms = await context.Rooms.ToListAsync();
        
        return mapper.Map<List<RoomDto>>(rooms);
    }

    public async Task<RoomDto> GetRoomAsync(Guid roomId)
    {
        var room =  await context.Rooms.FirstOrDefaultAsync(x => x.Id == roomId);
        
        return mapper.Map<RoomDto>(room);
    }

    public async Task<List<RoomDto>> BookingBusyTimeAsync(Guid id, DateTime date)
    {
        var rooms = await context.Rooms.Where(x => x.Id == id)
            .Include(x => x.Bookings.Where(b => b.StartTime.Date == date.Date))
            .ToListAsync();
        
        return mapper.Map<List<RoomDto>>(rooms);
    }
    
    public Task<RoomDto> CreateRoomAsync(CreateRoomDto dto)
    {
        try
        {
            var room = mapper.Map<Room>(dto);
            context.Add(room);
            context.SaveChanges();
        
            return Task.FromResult(mapper.Map<RoomDto>(room));
        }
        catch (Exception exception)
        {
            return Task.FromException<RoomDto>(exception);
        }
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