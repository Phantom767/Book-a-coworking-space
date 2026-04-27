using Coworking.Domain.Enums;

namespace Coworking.Domain.Entity;

public class Room : EntityBase
{
    public string Name { get; init; }
    public string Description { get; init; }
    public decimal PricePerHour { get; set; }
    public int Capacity { get; init; }
    public RoomType Type { get; init; }
    public List<string> Amenities { get; init; }
    public string? PhotoUrl { get; set; }
    public string? PhotoHash { get; set; }
    public virtual ICollection<Booking> Bookings { get; init; } = new List<Booking>();
    
    // Конструктор защищает инварианты
    public Room(string name, int capacity, decimal pricePerHour, string description, List<string> amenities) 
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required");
        if (pricePerHour < 0) throw new ArgumentException("Price cannot be negative");
        
        Name = name;    
        PricePerHour = pricePerHour;
        Capacity = capacity;
        Description = description;
        Amenities = amenities;
    }
    
    public void UpdatePrice(decimal newPrice) 
    {
        if (newPrice < 0) throw new ArgumentException("Price cannot be negative");
        PricePerHour = newPrice;
    }
}