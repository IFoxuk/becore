using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using becore.api.Models;
using becore.api.Services.Interfaces;

namespace becore.api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterAsync(request);

        if (result == null)
        {
            return BadRequest(new { message = "User with this username or email already exists" });
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(request);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid username/email or password" });
        }

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetCurrentUser()
    {
        // Логируем все claims для диагностики
        Console.WriteLine("=== GetCurrentUser DEBUG ===");
        Console.WriteLine($"User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
        Console.WriteLine($"User.Identity.AuthenticationType: {User.Identity?.AuthenticationType}");
        Console.WriteLine("All Claims:");
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"  {claim.Type}: {claim.Value}");
        }
        Console.WriteLine("=== END CLAIMS ===");
        
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null)
        {
            Console.WriteLine("GetCurrentUser: userIdClaim is null");
            // Пробуем альтернативные способы получения ID
            var altIdClaim = User.FindFirst("sub") ?? User.FindFirst("nameid");
            if (altIdClaim != null)
            {
                Console.WriteLine($"GetCurrentUser: Found alternative ID claim: {altIdClaim.Type} = {altIdClaim.Value}");
                userIdClaim = altIdClaim;
            }
            else
            {
                return Unauthorized(new { message = "User ID claim not found" });
            }
        }
        
        if (!Guid.TryParse(userIdClaim.Value, out var userId))
        {
            Console.WriteLine($"GetCurrentUser: Invalid user ID format: {userIdClaim.Value}");
            return Unauthorized(new { message = "Invalid user ID format" });
        }
        
        Console.WriteLine($"GetCurrentUser: Looking for user with ID: {userId}");
        var user = await _authService.GetUserByIdAsync(userId);

        if (user == null)
        {
            Console.WriteLine($"GetCurrentUser: User not found with ID: {userId}");
            
            // Дополнительная диагностика - попробуем найти пользователя по имени
            var usernameClaim = User.FindFirst(ClaimTypes.Name);
            if (usernameClaim != null)
            {
                Console.WriteLine($"GetCurrentUser: Trying to find user by username: {usernameClaim.Value}");
                var userByName = await _authService.GetUserByUsernameAsync(usernameClaim.Value);
                if (userByName != null)
                {
                    Console.WriteLine($"GetCurrentUser: Found user by username! ID mismatch - Token ID: {userId}, DB ID: {userByName.Id}");
                }
                else
                {
                    Console.WriteLine($"GetCurrentUser: User not found by username either: {usernameClaim.Value}");
                }
            }
            
            return NotFound(new { message = "User not found", userId = userId.ToString() });
        }
        
        Console.WriteLine($"GetCurrentUser: Found user: {user.Username} (ID: {user.Id})");
        return Ok(user);
    }

    [HttpGet("check-username/{username}")]
    public async Task<ActionResult> CheckUsername(string username)
    {
        var exists = await _authService.UserExistsAsync(username);
        return Ok(new { exists });
    }

    [HttpGet("check-email/{email}")]
    public async Task<ActionResult> CheckEmail(string email)
    {
        var exists = await _authService.UserExistsAsync(email);
        return Ok(new { exists });
    }
    
    [HttpGet("debug-auth")]
    public ActionResult DebugAuth()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        
        // Логируем все заголовки
        Console.WriteLine("=== ALL REQUEST HEADERS ===");
        foreach (var header in Request.Headers)
        {
            Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value.ToArray())}");
        }
        Console.WriteLine("=== END HEADERS ===");
        
        // Дополнительная диагностика JWT
        string jwtDiagnostic = "No JWT validation attempted";
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length);
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                
                jwtDiagnostic = $"JWT parsed successfully. Issuer: {jsonToken.Issuer}, Audience: {string.Join(", ", jsonToken.Audiences)}, Expires: {jsonToken.ValidTo}";
                
                Console.WriteLine($"JWT Token Claims:");
                foreach (var claim in jsonToken.Claims)
                {
                    Console.WriteLine($"  {claim.Type}: {claim.Value}");
                }
            }
            catch (Exception ex)
            {
                jwtDiagnostic = $"JWT parsing failed: {ex.Message}";
            }
        }
        
        var result = new 
        {
            HasAuthHeader = !string.IsNullOrEmpty(authHeader),
            AuthHeader = authHeader,
            JwtDiagnostic = jwtDiagnostic,
            AllHeaders = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray()),
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            AuthenticationType = User.Identity?.AuthenticationType,
            Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
            UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            
            // Дополнительная информация о конфигурации
            ExpectedIssuer = "becore-api",
            ExpectedAudience = "becore-client"
        };
        
        Console.WriteLine($"Debug Auth Result: {System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}");
        return Ok(result);
    }
}
