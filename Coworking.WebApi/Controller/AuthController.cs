using Coworking.Application.DTOs.Auth;
using Coworking.Application.Interfaces;
using Coworking.Domain.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Coworking.WebApi.Controller;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
    IAuthService authService, 
    SignInManager<ApplicationUser> signInManager, 
    UserManager<ApplicationUser> userManager,
    ILogger<AuthController> logger) : ApiController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        logger.LogInformation("API запрос: регистрация пользователя {Email}", request.Email);
        
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Невалидные данные при регистрации: {Email}", request.Email);
            return BadRequest(new { message = "Неверные данные" });
        }
        
        var result = await authService.RegisterAsync(request);
        return result.Match(
            authResponse => {
                logger.LogInformation("Пользователь успешно зарегистрирован через API: {Email}", request.Email);
                return Ok(authResponse);
            },
            errors => {
                logger.LogError("Ошибка регистрации для {Email}: {Errors}", 
                    request.Email, string.Join("; ", errors.Select(e => e.Description)));
                return Problem(
                    title: "Ошибка регистрации",
                    detail: string.Join("; ", errors.Select(e => e.Description)),
                    statusCode: 400
                );
            }
        );
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        logger.LogInformation("API запрос: вход пользователя {Email}", request.Email);
        
        // 1. Используем AuthService для проверки данных
        var result = await authService.LoginAsync(request);

        return await result.MatchAsync<IActionResult>(
            async authResponse => 
            {
                // 2. ВАЖНО: Находим пользователя и создаем КУКИ через SignInManager
                var user = await userManager.FindByEmailAsync(request.Email);
            
                // Это создаст Set-Cookie заголовок, который браузер сохранит сам
                if (user != null)
                {
                    await signInManager.SignInAsync(user, isPersistent: request.RememberMe);
                    logger.LogInformation("Пользователь успешно вошел через API: {Email}", request.Email);
                }

                return Ok(new { 
                    success = true, 
                    token = authResponse.Token, // JWT можно оставить для будущего мобильного приложения
                    redirectUrl = "/Index" 
                });
            },
            errors => {
                logger.LogWarning("Ошибка входа для {Email}: {Errors}", 
                    request.Email, string.Join("; ", errors.Select(e => e.Description)));
                return Task.FromResult<IActionResult>(Problem(
                    title: "Ошибка входа",
                    detail: string.Join("; ", errors.Select(e => e.Description)),
                    statusCode: 401
                ));
            }
        );
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userEmail = User.Identity?.Name ?? "Unknown";
        logger.LogInformation("API запрос: выход пользователя {Email}", userEmail);
        
        await signInManager.SignOutAsync();
        
        logger.LogInformation("Пользователь успешно вышел: {Email}", userEmail);
        
        return Ok(new { success = true });
    }
}