using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Coworking.Application.Interfaces;
using Coworking.Domain.Entity;
using Coworking.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Coworking.Infrastructure.Authentication;

public class JwtTokenGenerator(JwtSettings jwtSettings, UserManager<ApplicationUser> userManager) : IJwtTokenGenerator
{
    public async Task<string> GenerateToken (ApplicationUser user)
    {
        return await GenerateToken(user, null, null, null);
    }

    private async Task<string> GenerateToken(ApplicationUser user, string? email, Role? role, string? roomId)
    {
        var roles = await userManager.GetRolesAsync(user);
        var id = await userManager.GetUserIdAsync(user);
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
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, id)
        };
        
        // Добавляем роли в claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // 3. Настраиваем параметры токена
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryMinutes),
            signingCredentials: creds
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}