using Coworking.Domain.Entity;

namespace Coworking.Application.DTOs;

public class BookingDto : EntityBase
{
    public Guid UserId { get; set; }
    public Guid RoomId { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
}