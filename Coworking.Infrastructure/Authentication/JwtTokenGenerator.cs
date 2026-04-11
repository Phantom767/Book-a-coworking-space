using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Coworking.Application.Interfaces;
using Coworking.Domain.Entity;
using Coworking.Domain.Enums;
using Microsoft.IdentityModel.Tokens;

namespace Coworking.Infrastructure.Authentication;

public class JwtTokenGenerator(JwtSettings jwtSettings) : IJwtTokenGenerator
{
    public string GenerateToken(User user)
    {
        return GenerateToken(user, null, null, null);
    }

    public string GenerateToken(User user, string email)
    {
        return GenerateToken(user, email, null, null);
    }

    public string GenerateToken(User user, string email, Role role)
    {
        return GenerateToken(user, email, role, null);
    }

    public string GenerateToken(User user, string? email, Role? role, string? roomId)
    {
        // 1. Получаем секретный ключ из конфигурации
        var secretKey = jwtSettings.Secret;
        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("JWT Secret Key is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 2. Формируем Claims (утверждения о пользователе)
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, $"{Role.User}") // Или динамически из БД
        };

        // Добавляем роли
        if (role != null) claims.Add(new Claim(ClaimTypes.Role, $"{role}"));

        // 3. Настраиваем параметры токена
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryMinutes),
            signingCredentials: creds
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
        // // Логика генерации JWT токена с использованием _jwtSettings
        // // и включением userId, email, role и roomId в payload токена
        //
        // // Здесь должна быть реализация генерации токена (например, с помощью библиотеки System.IdentityModel.Tokens.Jwt)
        //
        // return "generated_jwt_token"; // Заглушка для примера
    }
}