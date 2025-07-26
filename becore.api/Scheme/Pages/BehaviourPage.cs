using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace becore.api.Scheme;

[Table("BehaviourPage")]
public class BehaviourPage: DbEntity
{
    public AdditionPage AdditionPage { get; set; } = null!;
    public List<BehaviourFile> Files { get; set; } = [];
}

[Table("BehaviourFile")]
public class BehaviourFile
{
    [Key] [Column("file")] public Guid Id { get; set; }
    [Key] [Required] [Column("behaviourPageId")] public Guid BehaviourPageId { get; set; }
    [Required] public BehaviourPage BehaviourPage { get; set; } = null!;
    [Required] [Column("uploaded")] public DateTime UploadDate { get; set; } = DateTime.Now;
    [Required] [Column("confirmed")] public bool Confirmed { get; set; } = false;
}