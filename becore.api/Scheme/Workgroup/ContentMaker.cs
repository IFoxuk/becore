using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using becore.api.Models;
using Microsoft.AspNetCore.Identity;

namespace becore.api.Scheme.Workgroup;

[Table("ContentMaker")]
public class ContentMaker: DbEntity
{
    [Required] [Column("userId")] public Guid UserId { get; set; }
    [Required] public ApplicationUser User { get; set; } = null!;
    public TeamMember? TeamMember { get; set; }
    public List<AdditionPage> Addons { get; set; } = [];
}