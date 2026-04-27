using Coworking.Application.DTOs.Auth;
using Coworking.Application.Interfaces;
using Coworking.Domain.Entity;
using Coworking.Domain.Enums;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Service;

public class AuthService(
    IJwtTokenGenerator jwtTokenGenerator,
    IApplicationDbContext context,
    UserManager<ApplicationUser> userManager)
    : IAuthService
{
    public async Task<ErrorOr<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        // Проверка на существование пользователя
        if (await context.Users.AnyAsync(u => u.Email == request.Email))
            return Error.Conflict("User.DuplicateEmail", "Пользователь с таким email уже существует.");

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
            return Error.Failure("User.CreationFailed", string.Join(", ", result.Errors.Select(e => e.Description)));

        // Назначаем роль "User" новому пользователю
        await userManager.AddToRoleAsync(user, user.RoleStatuse.ToString());

        // Генерация токена
        string token = await jwtTokenGenerator.GenerateToken(user);
        
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
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return Error.NotFound("User.NotFound", "Пользователь не найден.");

        // Проверка пароля через UserManager
        bool isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            return Error.Unauthorized("User.InvalidCredentials", "Неверный email или пароль.");

        string token = await jwtTokenGenerator.GenerateToken(user);
        
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