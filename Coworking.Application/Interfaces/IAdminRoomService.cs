using Coworking.Application.DTOs;
using ErrorOr;

namespace Coworking.Application.Interfaces;

public interface IAdminRoomService
{
    Task<ErrorOr<RoomDto>> CreateRoomAsync(CreateRoomDto dto);
    Task<RoomDto?> UpdatePriceAsync(Guid roomId, decimal price);
    Task<RoomDto> DeleteRoomAsync(Guid roomId);
}