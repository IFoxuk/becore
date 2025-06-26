using System.Security.Claims;
using becore.api.Models;

namespace becore.api.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user);
    ClaimsPrincipal? ValidateToken(string token);
    DateTime GetTokenExpiration();
}
