using becore.api.Models;

namespace becore.api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<UserResponse?> GetUserByIdAsync(Guid userId);
    Task<UserResponse?> GetUserByUsernameAsync(string username);
    Task<bool> UserExistsAsync(string usernameOrEmail);
}
