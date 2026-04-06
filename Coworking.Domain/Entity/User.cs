namespace CoworkingDomain.Entity;

public class User : EntityBase
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; } // Например: "Admin", "User"
}