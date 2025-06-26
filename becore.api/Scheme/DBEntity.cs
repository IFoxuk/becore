using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace becore.api.Scheme;

public abstract class DbEntity
{
    [Required] public Guid Id { get; set; }

    public override string ToString() => $"{Id}";
}