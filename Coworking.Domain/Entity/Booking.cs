
using Coworking.Domain.Enums;

namespace Coworking.Domain.Entity;

public class Booking : EntityBase
{
    public Guid UserId { get; set; }
    public Guid RoomId { get; set; }
    
    public virtual Room Room { get; set; } = null!;
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalPrice { get; set; }
    
    public BookingStatus Status { get; set; }
}