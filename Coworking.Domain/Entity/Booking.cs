
using Coworking.Domain.Enums;

namespace Coworking.Domain.Entity;

public sealed class Booking : EntityBase
{
    public Guid UserId { get; set; }
    public Guid RoomId { get; set; }
    
    public required string RoomName { get; set; }
    public User? User { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalPrice { get; set; }
    
    public BookingStatus Status { get; set; }
}