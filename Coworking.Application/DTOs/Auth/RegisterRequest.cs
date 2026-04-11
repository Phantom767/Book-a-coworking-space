using System.ComponentModel.DataAnnotations;

namespace Coworking.Application.DTOs.Auth;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}