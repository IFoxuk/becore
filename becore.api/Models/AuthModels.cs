using System.ComponentModel.DataAnnotations;

namespace becore.api.Models;

public class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string Username { get; set; }
    
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public required string Email { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string Password { get; set; }
}

public class LoginRequest
{
    [Required]
    public required string UsernameOrEmail { get; set; }
    
    [Required]
    public required string Password { get; set; }
}

public class AuthResponse
{
    public required string Token { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class UserResponse
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
}
