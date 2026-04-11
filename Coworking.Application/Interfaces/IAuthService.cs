using Coworking.Application.DTOs.Auth;
using ErrorOr;

namespace Coworking.Application.Interfaces;

public interface IAuthService
{
    Task<ErrorOr<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ErrorOr<AuthResponse>> LoginAsync(LoginRequest request);
}