using System.ComponentModel.DataAnnotations.Schema;
using becore.api.Scheme.Workgroup;
using Microsoft.AspNetCore.Identity;

namespace becore.api.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public ContentMaker? ContentMaker { get; set; }
}
