using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using becore.api.Models;

namespace becore.api.Scheme.System;

[Table("File")]
public class File : DbEntity
{
    [Required] [MaxLength(32)] public required string Type { get; set; }
    public long Size { get; set; }
    public ApplicationUser? User { get; set; }
}
