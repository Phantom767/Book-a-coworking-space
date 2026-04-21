using Coworking.Application.DTOs;
using Coworking.Infrastructure.Persistence;
using AutoMapper;
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
}