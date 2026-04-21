using Coworking.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Coworking.Domain.Entity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public Role RoleStatuse { get; init; } // Например: "Admin", "User"
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}