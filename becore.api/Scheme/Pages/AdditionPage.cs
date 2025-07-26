using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using becore.api.Scheme.Workgroup;

namespace becore.api.Scheme;

/// <summary>
/// Страница дополнения, содержащая в себе всю информацию о модификации.
/// </summary>
[Table("AddonPage")]
public class AdditionPage: DbEntity
{
    [Required] [Column("name")] [MaxLength(32)] public string Name { get; set; } = string.Empty;
    [Required] [Column("description")] [MaxLength(1024)] public string Description { get; set; } = string.Empty;
    public Guid? QuadIcon { get; set; }
    public Guid? WideIcon { get; set; }
    public ResourcePage? ResourcePage { get; set; }
    public BehaviourPage? BehaviourPage { get; set; }
    public PageStatus Status { get; set; } = PageStatus.Draft;
    [Column("teamId")] public Guid? TeamId { get; set; }
    public Team? Team { get; set; }
    [Column("contentMakerId")] public Guid? OwnerId { get; set; }
    public ContentMaker? Owner { get; set; }
    
    
    public AdditionPage() { }
    public AdditionPage(string name, string description)
    {
        Name=name;
        Description=description;
    }
    
}