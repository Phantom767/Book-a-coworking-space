using Coworking.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Coworking.Domain.Entity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public Role RoleStatuse { get; init; }
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    public bool EmailNotificationsEnabled { get; set; } = true;
    public DateTime? PasswordChangedAt { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}