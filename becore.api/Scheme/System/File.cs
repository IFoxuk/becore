using System.ComponentModel.DataAnnotations.Schema;

namespace becore.api.Scheme.System;

[Table("File")]
public class File : DbEntity
{
    public string? Type { get; set; }
    public User? User { get; set; }
}