using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

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
    public Guid ImageId { get; set; } = Guid.Empty;
    public Guid QuadImageId { get; set; } = Guid.Empty;
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
    
    public Guid? QuadIcon { get; set; }
    
    public Guid? WideIcon { get; set; }
    
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
    
    public Guid? QuadIcon { get; set; }
    
    public Guid? WideIcon { get; set; }
    
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
    
    public Guid? QuadIcon { get; set; }
    
    public Guid? WideIcon { get; set; }
    
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

/// <summary>
/// DTO для создания страницы с иконками через форму
/// </summary>
public class CreatePageWithIconsDto
{
    [Required]
    public Guid? userId { get; set; }

    /// <summary>
    /// Название страницы
    /// </summary>
    [Required]
    [MaxLength(32)]
    public required string Name { get; set; }
    
    /// <summary>
    /// Описание страницы
    /// </summary>
    [MaxLength(256)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Содержимое страницы
    /// </summary>
    [MaxLength(2048)]
    public string? Content { get; set; }
    
    /// <summary>
    /// Теги в виде строки, разделенной запятыми
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// Квадратная иконка
    /// </summary>
    public IFormFile? QuadIcon { get; set; }
    
    /// <summary>
    /// Широкая иконка
    /// </summary>
    public IFormFile? WideIcon { get; set; }
    
    /// <summary>
    /// Преобразует строку тегов в список
    /// </summary>
    public List<string> GetTagsList()
    {
        return string.IsNullOrEmpty(Tags) 
            ? new List<string>() 
            : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(t => t.Trim())
                  .Where(t => !string.IsNullOrEmpty(t))
                  .ToList();
    }
}

/// <summary>
/// DTO для обновления страницы с иконками через форму
/// </summary>
public class UpdatePageWithIconsDto
{
    /// <summary>
    /// Название страницы
    /// </summary>
    [Required]
    [MaxLength(32)]
    public required string Name { get; set; }
    
    /// <summary>
    /// Описание страницы
    /// </summary>
    [MaxLength(256)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Содержимое страницы
    /// </summary>
    [MaxLength(2048)]
    public string? Content { get; set; }
    
    /// <summary>
    /// Теги в виде строки, разделенной запятыми
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// Квадратная иконка
    /// </summary>
    public IFormFile? QuadIcon { get; set; }
    
    /// <summary>
    /// Широкая иконка
    /// </summary>
    public IFormFile? WideIcon { get; set; }
    
    /// <summary>
    /// Флаг замены квадратной иконки
    /// </summary>
    public bool ReplaceQuadIcon { get; set; } = false;
    
    /// <summary>
    /// Флаг замены широкой иконки
    /// </summary>
    public bool ReplaceWideIcon { get; set; } = false;
    
    /// <summary>
    /// Преобразует строку тегов в список
    /// </summary>
    public List<string> GetTagsList()
    {
        return string.IsNullOrEmpty(Tags) 
            ? new List<string>() 
            : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(t => t.Trim())
                  .Where(t => !string.IsNullOrEmpty(t))
                  .ToList();
    }
}

/// <summary>
/// DTO для загрузки иконок на существующую страницу
/// </summary>
public class UploadIconsDto
{
    /// <summary>
    /// Квадратная иконка
    /// </summary>
    public IFormFile? QuadIcon { get; set; }
    
    /// <summary>
    /// Широкая иконка
    /// </summary>
    public IFormFile? WideIcon { get; set; }
    
    /// <summary>
    /// Проверяет, есть ли файлы для загрузки
    /// </summary>
    public bool HasFilesToUpload => QuadIcon != null || WideIcon != null;
}

/// <summary>
/// DTO для информации о файле
/// </summary>
public class FileInfoDto
{
    public Guid Id { get; set; }
    public string? Type { get; set; }
    public long Size { get; set; }
    public string? Url { get; set; }
}
