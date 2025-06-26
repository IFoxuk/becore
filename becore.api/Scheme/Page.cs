using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using becore.api.Scheme.Packs;

namespace becore.api.Scheme;

[Table("Page")]
public class Page : DbEntity
{
    [Required] [MaxLength(32)] public required string Name { get; set; }
    [MaxLength(256)] public string? Description { get; set; }
    [MaxLength(2048)] public string? Content { get; set; }
    public string? QuadIcon { get; set; }
    public string? WideIcon { get; set; }
    [NotMapped] public List<Pack> Packs { get; set; } = [];
    
    // Навигационные свойства для тегов
    public virtual ICollection<PageTag> PageTags { get; set; } = new List<PageTag>();
    
    // Удобное свойство для работы с тегами как со строками
    [NotMapped]
    public List<string> Tags 
    {
        get => PageTags.Select(pt => pt.TagName).ToList();
        set
        {
            PageTags.Clear();
            foreach (var tag in value)
            {
                PageTags.Add(new PageTag { TagName = tag, PageId = Id });
            }
        }
    }
}