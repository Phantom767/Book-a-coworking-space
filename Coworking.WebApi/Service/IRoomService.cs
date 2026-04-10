using Coworking.Application.DTOs;

namespace Coworking.WebApi.Service;

public interface IRoomService
{
    public Task<List<RoomDto>> GetAllRoomsAsync();
    public Task<RoomDto> GetRoomAsync(Guid roomId);
    public Task<RoomDto> CreateRoomAsync(CreateRoomDto dto);
    public Task<RoomDto?> UpdatePriceAsync(Guid roomId, decimal price);
    public Task<RoomDto> DeleteRoomAsync(Guid roomId);
}