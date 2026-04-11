using Coworking.Application.DTOs.Auth;
using Coworking.Application.Interfaces;
using Coworking.Domain.Entity;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Service;

public class AuthService(
    IJwtTokenGenerator jwtTokenGenerator,
    IApplicationDbContext context)
    : IAuthService
{
    public async Task<ErrorOr<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        // Проверка на существование пользователя
        if (await context.Users.AnyAsync(u => u.Email == request.Email))
            return Error.Conflict("User.DuplicateEmail", "Пользователь с таким email уже существует.");

        // Хеширование пароля
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName ?? "",
            LastName = request.LastName ?? "",
            Email = request.Email,
            PasswordHash = passwordHash,
            CreationTime = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Генерация токена
        var token = jwtTokenGenerator.GenerateToken(user);
        
        var authResponse = new AuthResponse
        {
            UserId = user.Id.ToString(),
            Email = user.Email,
            Token = token,
            Expiration = DateTime.UtcNow.AddHours(24) // или бери из JWT
        };

        return authResponse;
    }

    public async Task<ErrorOr<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return Error.NotFound("User.NotFound", "Пользователь не найден.");

        // Проверка пароля
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
            return Error.Unauthorized("User.InvalidCredentials", "Неверный email или пароль.");

        var token = jwtTokenGenerator.GenerateToken(user);
        
        var authResponse = new AuthResponse
        {
            UserId = user.Id.ToString(),
            Email = user.Email,
            Token = token,
            Expiration = DateTime.UtcNow.AddHours(24) // или бери из JWT
        };

        return authResponse;
    }
}