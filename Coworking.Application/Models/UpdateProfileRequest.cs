using System.ComponentModel.DataAnnotations;

namespace Coworking.Application.Models;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Введите имя")]
    [StringLength(50, ErrorMessage = "Имя не может быть длиннее 50 символов")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите фамилию")]
    [StringLength(50, ErrorMessage = "Фамилия не может быть длиннее 50 символов")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите Email")]
    [EmailAddress(ErrorMessage = "Некорректный формат Email")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Некорректный формат телефона")]
    public string? Phone { get; set; }
        
    // Это поле мы не обновляем через форму профиля обычно, 
    // но если нужно, можно добавить. Чаще всего настройки уведомлений выносят отдельно.
    public bool EmailNotificationsEnabled { get; set; } 
}