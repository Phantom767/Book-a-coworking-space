using Coworking.Application.DTOs;
using Coworking.Application.DTOs.Auth;

namespace Coworking.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
}