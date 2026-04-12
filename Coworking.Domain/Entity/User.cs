using Coworking.Domain.Enums;

namespace Coworking.Domain.Entity;

public class User : EntityBase
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public Role RoleStatuseType { get; init; } // Например: "Admin", "User"
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}