using Coworking.Application.Models;
using Coworking.Domain.Entity;

namespace Coworking.Application.Interfaces;

public interface IProfileService
{
    // Получение данных для отображения профиля (можно вернуть DTO, а не Entity)
    Task<ApplicationUser> GetUserProfileAsync(Guid userId);
        
    // Обновление профиля
    Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
        
    // Переключение уведомлений
    Task ToggleEmailNotificationsAsync(Guid userId, bool isEnabled);
        
    // Получение даты смены пароля (если это бизнес-логика)
    Task<DateTime?> GetPasswordChangedDateAsync(Guid userId);
}