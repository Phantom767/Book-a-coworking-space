using Coworking.Application.Interfaces;
using Coworking.Application.Models;
using Coworking.Domain.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Coworking.Application.Service;

public class ProfileService(
    UserManager<ApplicationUser> userManager,
    ILogger<ProfileService> logger) : IProfileService
{
    public async Task<ApplicationUser> GetUserProfileAsync(Guid userId)
    {
        logger.LogInformation("Получение профиля пользователя: {UserId}", userId);
        
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            logger.LogWarning("Пользователь не найден: {UserId}", userId);
            throw new InvalidOperationException("User not found");
        }
        
        return user;
    }

    public async Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        logger.LogInformation("Обновление профиля пользователя: {UserId}", userId);
        
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            logger.LogWarning("Пользователь не найден при обновлении профиля: {UserId}", userId);
            return false;
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        
        var emailResult = await userManager.SetEmailAsync(user, request.Email);
        if (!emailResult.Succeeded)
        {
            logger.LogError("Ошибка при установке email: {Email} для пользователя {UserId}", 
                request.Email, userId);
            return false;
        }

        user.PhoneNumber = request.Phone;
        
        var result = await userManager.UpdateAsync(user);
        
        if (result.Succeeded)
            logger.LogInformation("Профиль пользователя успешно обновлен: {UserId}", userId);
        else
            logger.LogError("Ошибка при обновлении профиля пользователя: {UserId}", userId);
        
        return result.Succeeded;
    }

    public async Task ToggleEmailNotificationsAsync(Guid userId, bool isEnabled)
    {
        logger.LogInformation("Изменение уведомлений по email для пользователя {UserId}: {IsEnabled}", 
            userId, isEnabled);
        
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            logger.LogWarning("Пользователь не найден: {UserId}", userId);
            return;
        }

        user.EmailNotificationsEnabled = isEnabled;
        await userManager.UpdateAsync(user);
        
        logger.LogInformation("Уведомления по email для пользователя {UserId} обновлены", userId);
    }

    public Task<DateTime?> GetPasswordChangedDateAsync(Guid userId)
    {
        logger.LogInformation("Получение даты последнего изменения пароля для пользователя: {UserId}", userId);
        
        var user = userManager.Users.FirstOrDefault(u => u.Id == userId);
        return Task.FromResult(user?.PasswordChangedAt);
    }
}
