using Coworking.Domain.Entity;
using Coworking.Domain.Enums;

namespace Coworking.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
    string GenerateToken(User user, string email);

    /// <summary>
    /// Генерирует JWT токен для пользователя
    /// </summary>
    /// <param name="user">пользователя</param>
    /// <param name="email">Email пользователя (для claims)</param>
    /// <param name="role">Список ролей пользователя</param>
    /// <returns>Строка JWT токена</returns>
    string GenerateToken(User user, string email, Role role);
    string GenerateToken(User user, string? email, Role? role, string? roomId);
}