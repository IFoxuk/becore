using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using becore.shared.DTOs;

namespace becore.api.Scheme;

[Table("PageTag")]
public class PageTag
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid PageId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public required string TagName { get; set; }
    
    // Навигационные свойства
    [ForeignKey("PageId")]
    public virtual Page Page { get; set; } = null!;

    public static implicit operator TagDto(PageTag tag) => new()
    {
        Id = tag.Id,
        Name = tag.TagName
    };
}
