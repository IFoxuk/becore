using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace becore.api.Scheme;

[Table("ResourcePage")]
public class ResourcePage: DbEntity
{
    public AdditionPage AdditionPage { get; set; } = null!;
    [NotMapped] private ushort? _resolution;

    [Column("resolution")]
    public ushort? Resolution
    {
        get => _resolution;
        set
        {
            if (value is null)
            {
                _resolution = null;
                return;
            }
            
            var v = Math.Max((int)value, 8);
            _resolution = (ushort)(1 << ((int)Math.Log(v - 1, 2) + 1));
        }
    }
    public List<ResourceFile> Files { get; set; } = [];
}

[Table("ResourceFile")]
public class ResourceFile
{ // Можно отбирать в файлах сразу по-данному guid в файловом хранилище.
    [Key] [Column("file")] public Guid Id { get; set; }
    [Key] [Required] [Column("resourcePageId")] public Guid ResourcePageId { get; set; } 
    [Required] public ResourcePage ResourcePage { get; set; } = null!;
    [Required] [Column("uploaded")] public DateTime UploadDate { get; set; } = DateTime.Now;
    [Required] [Column("confirmed")] public bool Confirmed { get; set; } = false;
}