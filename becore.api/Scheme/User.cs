using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace becore.api.Scheme;

[Table("Users")]
public class User : DbEntity
{
    [Required]
    [MaxLength(50)]
    public required string Username { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string Email { get; set; }
    
    [Required]
    public required string PasswordHash { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    public bool IsActive { get; set; } = true;
}
