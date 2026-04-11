using Coworking.Domain.Entity;

namespace Coworking.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
    string GenerateToken(User user, string email);
    
    /// <summary>
    /// Генерирует JWT токен для пользователя
    /// </summary>
    /// <param name="userId">ID пользователя (GUID или string)</param>
    /// <param name="email">Email пользователя (для claims)</param>
    /// <param name="roles">Список ролей пользователя</param>
    /// <returns>Строка JWT токена</returns>
    string GenerateToken(User user, string email, string role);
    string GenerateToken(User user, string email, string role, string roomId);
}