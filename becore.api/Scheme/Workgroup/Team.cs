using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace becore.api.Scheme.Workgroup;

[Table("Team")]
public class Team: DbEntity
{
    [Required] [Column("name")] [MaxLength(32)] public string Name { get; set; } = string.Empty;
    [Required] [Column("description")] [MaxLength(1024)] public string Description { get; set; } = string.Empty;
    public List<TeamMember> Members { get; set; } = [];
    public List<AdditionPage> Addons { get; set; } = [];
}