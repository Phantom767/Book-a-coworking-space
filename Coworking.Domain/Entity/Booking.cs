
using Coworking.Domain.Enums;
using CoworkingDomain.Entity;

namespace Coworking.Domain.Entity;

public class Booking : EntityBase
{
    public Guid UserId { get; set; }
    public Guid RoomId { get; set; }
    
    public virtual Room Room { get; set; } = null!;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    
    public BookingStatus Status { get; set; }
}