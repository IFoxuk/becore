using System.ComponentModel.DataAnnotations.Schema;

namespace becore.api.Scheme;

public abstract class DbEntity
{
    public required Guid Id { get; set; }
}