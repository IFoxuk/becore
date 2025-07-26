using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace becore.api.Scheme.Workgroup;

[Table("TeamMember")]
public class TeamMember: DbEntity
{
    [Required] public ContentMaker ContentMaker { get; set; } = null!;
    [Required] [Column("teamId")] public Guid TeamId { get; set; }
    [Required] public Team Team { get; set; } = null!;
    [Required] [Column("position")] public TeamPosition Position { get; set; } = TeamPosition.Quest;
}