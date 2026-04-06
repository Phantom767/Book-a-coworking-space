using Coworking.Domain.Enums;
using CoworkingDomain.Entity;

namespace Coworking.Domain.Entity;

public class Room : EntityBase
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal PricePerHour { get; set; }
    public int Capacity { get; set; }
    public RoomType Type { get; set; }
    public List<string> Amenities { get; set; }
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    
    // Конструктор защищает инварианты
    public Room(string name, int capacity, decimal price, string description, List<string> amenities) 
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required");
        if (price < 0) throw new ArgumentException("Price cannot be negative");
        
        Name = name;    
        PricePerHour = price;
        Capacity = capacity;
        Description = description;
        Amenities = amenities;
    }
    
    public void UpdatePrice(decimal newPrice) 
    {
        // if (newPrice < 0) throw new DomainException("Price cannot be negative");
        PricePerHour = newPrice;
    }
}