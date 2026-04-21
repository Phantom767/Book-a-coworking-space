using Coworking.Domain.Entity;

namespace Coworking.Application.Interfaces;

public interface IJwtTokenGenerator
{
    Task<string> GenerateToken (ApplicationUser user);
}