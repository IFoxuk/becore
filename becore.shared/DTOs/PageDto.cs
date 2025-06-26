using System.ComponentModel.DataAnnotations;

namespace becore.shared.DTOs;

/// <summary>
/// DTO для отображения страниц контента в интерфейсе
/// </summary>
public partial class PageDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public int ViewCount { get; set; }
    public int DownloadCount { get; set; }
}

/// <summary>
/// DTO для API страниц (соответствует существующему API)
/// </summary>
public partial class ApiPageDto
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
}

/// <summary>
/// DTO для ответа с пагинацией
/// </summary>
public class SearchResultDto
{
    public List<PageDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>
/// DTO для фильтрации страниц
/// </summary>
public class PageFilterDto
{
    public string? Name { get; set; }
    public string? Tag { get; set; }
}
