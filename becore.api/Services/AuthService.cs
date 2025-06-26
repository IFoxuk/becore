using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using becore.api.Scheme;
using becore.api.Models;
using becore.api.Services.Interfaces;

namespace becore.api.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;

    public AuthService(
        ApplicationContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        // Создаем нового пользователя для Identity
        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        // Создаем пользователя через Identity
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return null; // Ошибка создания пользователя
        }

        // Обновляем время последнего входа
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Генерируем токен
        var token = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            Username = user.UserName!,
            Email = user.Email!,
            ExpiresAt = _jwtService.GetTokenExpiration()
        };
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        // Ищем пользователя по имени или email
        ApplicationUser? user = null;
        
        if (request.UsernameOrEmail.Contains('@'))
        {
            user = await _userManager.FindByEmailAsync(request.UsernameOrEmail);
        }
        else
        {
            user = await _userManager.FindByNameAsync(request.UsernameOrEmail);
        }

        if (user == null || !user.IsActive)
        {
            return null; // Пользователь не найден
        }

        // Проверяем пароль через Identity
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        
        if (!result.Succeeded)
        {
            return null; // Неверный пароль
        }

        // Обновляем время последнего входа
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Генерируем токен
        var token = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            Username = user.UserName!,
            Email = user.Email!,
            ExpiresAt = _jwtService.GetTokenExpiration()
        };
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null || !user.IsActive)
        {
            return null;
        }

        return new UserResponse
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive
        };
    }

    public async Task<bool> UserExistsAsync(string usernameOrEmail)
    {
        if (usernameOrEmail.Contains('@'))
        {
            var userByEmail = await _userManager.FindByEmailAsync(usernameOrEmail);
            return userByEmail != null;
        }
        else
        {
            var userByName = await _userManager.FindByNameAsync(usernameOrEmail);
            return userByName != null;
        }
    }
    
    public async Task<UserResponse?> GetUserByUsernameAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);

        if (user == null || !user.IsActive)
        {
            return null;
        }

        return new UserResponse
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive
        };
    }
}
