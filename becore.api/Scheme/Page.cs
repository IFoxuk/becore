using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using becore.api.Scheme.Packs;
using becore.shared.DTOs;

namespace becore.api.Scheme;

[Table("Page")]
public class Page : DbEntity
{
    [Required] [MaxLength(32)] public required string Name { get; set; }
    [MaxLength(256)] public string? Description { get; set; }
    [MaxLength(2048)] public string? Content { get; set; }
    public Guid? QuadIcon { get; set; }
    public Guid? WideIcon { get; set; }
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
    
    // Implicit операторы для работы с shared DTO
    
    /// <summary>
    /// Конвертация Page в PageDto для клиента
    /// </summary>
    public static implicit operator PageDto(Page page)
    {
        return new PageDto
        {
            Id = page.Id.GetHashCode(), // Конвертируем Guid в int для клиента
            Title = page.Name,
            Author = "Unknown", // TODO: добавить поле Author в модель Page
            Description = page.Description ?? string.Empty,
            Content = page.Content ?? string.Empty,
            ImageId = page.QuadIcon ?? Guid.Empty,
            QuadImageId = page.WideIcon ?? Guid.Empty,
            Tags = page.Tags,
            CreatedAt = DateTime.Now, // TODO: добавить поле CreatedAt в модель Page
            ViewCount = 0, // TODO: добавить поля статистики в модель Page
            DownloadCount = 0
        };
    }
    
    /// <summary>
    /// Конвертация CreatePageDto в Page
    /// </summary>
    public static implicit operator Page(CreatePageDto createDto)
    {
        var page = new Page
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Description = createDto.Description,
            QuadIcon = createDto.QuadIcon,
            WideIcon = createDto.WideIcon
        };
        
        // Устанавливаем теги после создания объекта, чтобы PageId был корректным
        foreach (var tag in createDto.Tags)
        {
            page.PageTags.Add(new PageTag { TagName = tag, PageId = page.Id });
        }
        
        return page;
    }
    
    /// <summary>
    /// Обновляет текущую Page из UpdatePageDto
    /// </summary>
    public void UpdateFromDto(UpdatePageDto updateDto)
    {
        Name = updateDto.Name;
        Description = updateDto.Description;
        QuadIcon = updateDto.QuadIcon;
        WideIcon = updateDto.WideIcon;
        Tags = updateDto.Tags;
    }
}
