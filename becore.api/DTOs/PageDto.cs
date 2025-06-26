using System.ComponentModel.DataAnnotations;
using becore.api.Scheme;

namespace becore.api.DTOs;

/// <summary>
/// DTO для возвращения данных о странице
/// </summary>
public class PageDto
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(32)]
    public required string Name { get; set; }
    
    [MaxLength(256)]
    public string? Description { get; set; }
    
    public string? QuadIcon { get; set; }
    
    public string? WideIcon { get; set; }
    
    public List<string> Tags { get; set; } = new List<string>();

    // Implicit operator для конвертации Page в PageDto
    public static implicit operator PageDto(Page page)
    {
        return new PageDto
        {
            Id = page.Id,
            Name = page.Name,
            Description = page.Description,
            QuadIcon = page.QuadIcon,
            WideIcon = page.WideIcon,
            Tags = page.Tags
        };
    }
}

/// <summary>
/// DTO для создания новой страницы (без Id)
/// </summary>
public class CreatePageDto
{
    [Required]
    [MaxLength(32)]
    public required string Name { get; set; }
    
    [MaxLength(256)]
    public string? Description { get; set; }
    
    public string? QuadIcon { get; set; }
    
    public string? WideIcon { get; set; }
    
    public List<string> Tags { get; set; } = new List<string>();

    // Implicit operator для конвертации CreatePageDto в Page
    public static implicit operator Page(CreatePageDto createDto)
    {
        return new Page
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Description = createDto.Description,
            QuadIcon = createDto.QuadIcon,
            WideIcon = createDto.WideIcon,
            Tags = createDto.Tags
        };
    }
}

/// <summary>
/// DTO для обновления страницы (без Id, он передается в URL)
/// </summary>
public class UpdatePageDto
{
    [Required]
    [MaxLength(32)]
    public required string Name { get; set; }
    
    [MaxLength(256)]
    public string? Description { get; set; }
    
    public string? QuadIcon { get; set; }
    
    public string? WideIcon { get; set; }
    
    public List<string> Tags { get; set; } = new List<string>();

    // Extension method для обновления существующей Page
    public void UpdatePage(Page page)
    {
        page.Name = this.Name;
        page.Description = this.Description;
        page.QuadIcon = this.QuadIcon;
        page.WideIcon = this.WideIcon;
        page.Tags = this.Tags;
    }
}
