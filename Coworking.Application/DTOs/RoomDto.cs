using Coworking.Domain.Entity;

namespace Coworking.Application.DTOs;

public class RoomDto : EntityBase
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal PricePerHour { get; set; }
    public int Capacity { get; set; }
    public List<string>? Amenities { get; set; }
    public virtual ICollection<Booking> Bookings { get; init; } = new List<Booking>();
}