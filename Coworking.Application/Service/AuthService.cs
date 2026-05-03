using Coworking.Application.DTOs.Auth;
using Coworking.Application.Interfaces;
using Coworking.Domain.Entity;
using Coworking.Domain.Enums;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Coworking.Application.Service;

public class AuthService(
    IJwtTokenGenerator jwtTokenGenerator,
    IApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    ILogger<AuthService> logger)
    : IAuthService
{
    public async Task<ErrorOr<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        logger.LogInformation("Попытка регистрации пользователя с email: {Email}", request.Email);
        
        // Проверка на существование пользователя
        if (await context.Users.AnyAsync(u => u.Email == request.Email))
        {
            logger.LogWarning("Попытка регистрации с уже используемым email: {Email}", request.Email);
            return Error.Conflict("User.DuplicateEmail", "Пользователь с таким email уже существует.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,        // ← ИСПРАВЛЕНО: Email обязателен для JWT-клейма
            FirstName = request.FirstName ?? "",
            LastName = request.LastName ?? "",
            CreationTime = DateTime.UtcNow,
            RoleStatuse = Role.User
        };

        // Создаем пользователя с хешированием пароля через UserManager
        var result = await userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
        {
            logger.LogError("Ошибка при создании пользователя {Email}: {Errors}", 
                request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return Error.Failure("User.CreationFailed", string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // Назначаем роль "User" новому пользователю
        await userManager.AddToRoleAsync(user, user.RoleStatuse.ToString());

        // Генерация токена
        string token = await jwtTokenGenerator.GenerateToken(user);
        
        logger.LogInformation("Пользователь успешно зарегистрирован: {Email}", request.Email);
        
        var authResponse = new AuthResponse
        {
            UserId = user.Id.ToString(),
            Email = request.Email,
            Token = token,
            Expiration = DateTime.UtcNow.AddHours(24)
        };

        return authResponse;
    }

    public async Task<ErrorOr<AuthResponse>> LoginAsync(LoginRequest request)
    {
        logger.LogInformation("Попытка входа пользователя с email: {Email}", request.Email);
        
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            logger.LogWarning("Попытка входа с несуществующим email: {Email}", request.Email);
            return Error.NotFound("User.NotFound", "Пользователь не найден.");
        }

        // Проверка пароля через UserManager
        bool isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            logger.LogWarning("Неверный пароль для пользователя: {Email}", request.Email);
            return Error.Unauthorized("User.InvalidCredentials", "Неверный email или пароль.");
        }

        string token = await jwtTokenGenerator.GenerateToken(user);
        
        logger.LogInformation("Пользователь успешно вошел: {Email}", request.Email);
        
        var authResponse = new AuthResponse
        {
            UserId = user.Id.ToString(),
            Email =  request.Email,
            Token = token,
            Expiration = DateTime.UtcNow.AddHours(24)
        };

        return authResponse;
    }
}