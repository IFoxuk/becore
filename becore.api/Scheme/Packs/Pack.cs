using System.ComponentModel.DataAnnotations;

namespace becore.api.Scheme.Packs;

public abstract class Pack : DbEntity
{
    [Required] [MaxLength(32)] public required string Name { get; set; }
    [MaxLength(256)] public string? Description { get; set; }
    [Required] public required PackType PackType { get; set; }
    [Required] public required DateTime Created { get; init; } = DateTime.Now;
    [Required] public Guid PageId { get; set; }
    [Required] public required Page Page { get; set; }
}