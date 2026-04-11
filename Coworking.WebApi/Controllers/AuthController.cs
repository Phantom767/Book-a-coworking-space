using Coworking.Application.DTOs.Auth;
using Coworking.Application.Interfaces;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);
        return result.Match(
            authResponse => Ok(authResponse),
            errors => Problem(
                title: "Ошибка регистрации",
                detail: string.Join("; ", errors.Select(e => e.Description)),
                statusCode: 400
                )
            );
            // error => error.Type switch
            // {
            //     "User.DuplicateEmail" => Conflict(error.Description),
            //     _ => StatusCode(500, "Internal Server Error")
            // });
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        return result.Match(
            authResponse => Ok(authResponse),
            errors => Problem(
                title: "Ошибка входа",
                detail: string.Join("; ", errors.Select(e => e.Description)),
                statusCode: errors.First().Type == ErrorType.NotFound ? 404 : 401
            )
        );
            // error => error.Type switch
            // {
            //     "User.NotFound" => NotFound(error.Description),
            //     "User.InvalidCredentials" => Unauthorized(error.Description),
            //     _ => StatusCode(500, "Internal Server Error")
            // });
    }   
}