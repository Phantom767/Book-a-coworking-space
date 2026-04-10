using Coworking.Domain.Entity;

namespace Coworking.Application.DTOs;

public class CreateBookingDto : EntityBase
{
    public Guid UserId { get; set; }
    public Guid RoomId { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}