using Coworking.Domain.Entity;

namespace Coworking.Application.DTOs;

public class CreateRoomDto : EntityBase
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal PricePerHour { get; set; }
    public int Capacity { get; set; }
    public string? PhotoUrl { get; set; }
    public string? PhotoHash { get; set; }
    public List<string>? Amenities { get; set; }
}