using Coworking.Application.DTOs.Auth;
using Coworking.Application.Interfaces;
using Coworking.Domain.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.WebApi.Controller;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService, 
    SignInManager<ApplicationUser> signInManager, 
    UserManager<ApplicationUser> userManager ) : ApiController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Неверные данные" });
        var result = await authService.RegisterAsync(request);
        return result.Match(
            authResponse => Ok(authResponse),
            errors => Problem(
                title: "Ошибка регистрации",
                detail: string.Join("; ", errors.Select(e => e.Description)),
                statusCode: 400
                )
            );
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // 1. Используем AuthService для проверки данных
        var result = await authService.LoginAsync(request);

        return await result.MatchAsync<IActionResult>(
            async authResponse => 
            {
                // 2. ВАЖНО: Находим пользователя и создаем КУКИ через SignInManager
                var user = await userManager.FindByEmailAsync(request.Email);
            
                // Это создаст Set-Cookie заголовок, который браузер сохранит сам
                if (user != null) await signInManager.SignInAsync(user, isPersistent: request.RememberMe);

                return Ok(new { 
                    success = true, 
                    token = authResponse.Token, // JWT можно оставить для будущего мобильного приложения
                    redirectUrl = "/Index" 
                });
            },
            errors => Task.FromResult<IActionResult>(Problem(
                title: "Ошибка входа",
                detail: string.Join("; ", errors.Select(e => e.Description)),
                statusCode: 401
            ))
        );
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok(new { success = true });
    }
}