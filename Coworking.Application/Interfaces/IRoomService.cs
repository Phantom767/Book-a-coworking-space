using Coworking.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Coworking.Application.Interfaces;

public interface IRoomService
{
    public Task<List<RoomDto>> GetAllRoomsAsync();
    public Task<RoomDto> GetRoomAsync(Guid roomId);
    public Task<List<RoomDto>> BookingBusyTimeAsync(Guid id, DateTime date);
    Task<string> UploadRoomPhotoAsync(Guid roomId, IFormFile photo);
    Task<UpdateRoomDto?> GetByIdAsync(Guid id);
    Task UpdateAsync(UpdateRoomDto room);
}