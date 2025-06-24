using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using becore.api.Scheme.Packs;

namespace becore.api.Scheme;

[Table("Page")]
public class Page : DbEntity
{
    [Required] [MaxLength(32)] public required string Name { get; set; }
    [MaxLength(256)] public string? Description { get; set; }
    public string? QuadIcon { get; set; }
    public string? WideIcon { get; set; }
    [NotMapped] public List<Pack> Packs { get; set; } = [];
}