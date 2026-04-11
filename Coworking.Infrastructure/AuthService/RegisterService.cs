using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Coworking.Domain.Entity;
using Coworking.Infrastructure.Persistence;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Infrastructure.AuthService;

public class RegisterService(ApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator)
{
    public async Task<ErrorOr<string>> RegisterAsync(RegisterDto dto)
    {
        // 1. Проверяем, нет ли уже такого Email
        if (await context.Users.AnyAsync(u => u.Email == dto.Email))
            return Error.Conflict("User.DuplicateEmail", "Этот Email уже занят.");

        // 2. Хешируем пароль
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // 3. Создаем пользователя
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName ?? "",
            LastName = dto.LastName ?? "",
            Email = dto.Email,
            PasswordHash = passwordHash,
            CreationTime = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // 4. Генерируем токен
        return jwtTokenGenerator.GenerateToken(user);
    }
}